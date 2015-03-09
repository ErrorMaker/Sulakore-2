using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Sulakore.Habbo;

namespace Sulakore.Communication
{
    public static class Eavesdropper
    {
        /// <summary>
        /// Gets the port that the proxy is currently listening to.
        /// </summary>
        public static int Port { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the proxy is currently running.
        /// </summary>
        public static bool IsRunning { get; private set; }

        /// <summary>
        /// Gets or sets a value that determines whether to apply the "no-cache, no-store" policy to the requests/responses.
        /// </summary>
        public static bool IsCacheDisabled { get; set; }

        public delegate void EavesdropperRequestEventHandler(EavesdropperRequestEventArgs e);
        public static event EavesdropperRequestEventHandler EavesdropperRequest;
        private static void OnEavesdropperRequest(EavesdropperRequestEventArgs e)
        {
            EavesdropperRequestEventHandler handler = EavesdropperRequest;
            if (handler != null) handler(e);
        }

        public delegate void EavesdropperResponseEventHandler(EavesdropperResponseEventArgs e);
        public static event EavesdropperResponseEventHandler EavesdropperResponse;
        private static void OnEavesdropperResponse(EavesdropperResponseEventArgs e)
        {
            EavesdropperResponseEventHandler handler = EavesdropperResponse;
            if (handler != null) handler(e);
        }

        private static int _processingRequests;

        private static readonly TcpListenerEx _listener;
        private static readonly char[] _commandSplit, _headerPairSplit;

        static Eavesdropper()
        {
            _headerPairSplit = new[] { ':', ' ' };
            _commandSplit = new[] { '\r', '\n' };

            _listener = new TcpListenerEx(IPAddress.Any, 0);
        }

        /// <summary>
        /// Begins running the proxy on a randomly chosen port.
        /// </summary>
        public static void Initiate()
        {
            Terminate();

            IsRunning = true;
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            NativeMethods.EnableProxy(Port);
            _listener.BeginAcceptSocket(RequestIntercepted, null);
        }
        /// <summary>
        /// Waits for any pending request to finish to proceed to terminate the proxy, and resets the machines proxy settings back to normal.
        /// </summary>
        public static void Terminate()
        {
            while (_processingRequests > 0) Thread.Sleep(400);

            IsRunning = false;
            if (_listener.Active)
            {
                _listener.Stop();
                Port = 0;
            }
            NativeMethods.DisableProxy();
            _processingRequests = 0;
        }

        private static void RequestIntercepted(IAsyncResult ar)
        {
            bool shouldTerminate = false;
            try
            {
                _processingRequests++;
                using (Socket requestSocket = _listener.EndAcceptSocket(ar))
                {
                    if (IsRunning)
                        _listener.BeginAcceptSocket(RequestIntercepted, null);

                    // Intercept the HTTP/HTTPS command from the local machine.
                    byte[] requestCommandBuffer = new byte[8192];
                    int length = requestSocket.Receive(requestCommandBuffer);

                    if (length == 0) return;
                    byte[] requestCommand = new byte[length];
                    Buffer.BlockCopy(requestCommandBuffer, 0, requestCommand, 0, length);

                    // Create a WebRequest instance using the intercepted commands/headers.
                    byte[] payload = null;
                    HttpWebRequest request = GetRequest(requestCommand, ref payload);

                    // Attempt to retrieve more data if available from the current stream.
                    if (requestSocket.Available == request.ContentLength
                        && payload == null)
                    {
                        payload = new byte[request.ContentLength];
                        requestSocket.Receive(payload);
                    }

                    // Notify the subscriber that a request has been constructed, and is ready to be sent.
                    if (EavesdropperRequest != null)
                    {
                        var e = new EavesdropperRequestEventArgs(request);

                        if (payload != null)
                            e.Payload = payload;

                        OnEavesdropperRequest(e);
                        if (e.Cancel) return;

                        payload = e.Payload;
                    }

                    switch (request.Method)
                    {
                        case "CONNECT": return; // Let's focus on HTTP for now.
                        case "POST": // Send the request data to the server.
                        {
                            using (Stream requestStream = request.GetRequestStream())
                                requestStream.Write(payload, 0, payload.Length);
                            break;
                        }
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (IsCacheDisabled)
                            response.Headers["Cache-Control"] = "no-cache, no-store";

                        byte[] responseData = new byte[0];
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            int readDataLength = 0;
                            byte[] responseDataBuffer = null;
                            byte[] readData = new byte[response.ContentLength < 0 ? 8192 : response.ContentLength];
                            while ((readDataLength = responseStream.Read(readData, 0, readData.Length)) > 0)
                            {
                                responseDataBuffer = new byte[responseData.Length + readDataLength];
                                Buffer.BlockCopy(responseData, 0, responseDataBuffer, 0, responseData.Length);
                                Buffer.BlockCopy(readData, 0, responseDataBuffer, responseData.Length, readDataLength);
                                responseData = responseDataBuffer;
                            }
                        }

                        // Notify the subscriber that a response has been intercepted, and is ready to be viewed.
                        if (EavesdropperResponse != null)
                        {
                            var e = new EavesdropperResponseEventArgs(response);
                            e.Payload = responseData;

                            OnEavesdropperResponse(e);

                            if (shouldTerminate = e.ShouldTerminate)
                                IsRunning = false;

                            if (e.Cancel) return;

                            responseData = e.Payload;
                        }

                        // Reply with the server's response back to the client. 
                        byte[] commandResponse = GetCommandResponse(request, response, responseData.Length);
                        requestSocket.Send(commandResponse);

                        if (responseData != null)
                            requestSocket.Send(responseData);
                    }
                }
            }
            catch { }
            finally
            {
                --_processingRequests;
                if (shouldTerminate)
                {
                    if (_listener.Active)
                    {
                        _listener.Stop();
                        Port = 0;
                    }
                    NativeMethods.DisableProxy();
                    _processingRequests = 0;
                }
            }
        }

        private static byte[] GetCommandResponse(HttpWebRequest request, HttpWebResponse response, int payloadSize)
        {
            string commandResponse = string.Format("HTTP/{0} {1} {2}\r\n{3}",
                response.ProtocolVersion.ToString(), (int)response.StatusCode, response.StatusDescription, response.Headers.ToString());

            return Encoding.ASCII.GetBytes(commandResponse);
        }
        private static HttpWebRequest GetRequest(byte[] data, ref byte[] payload)
        {
            string[] commands = Encoding.ASCII.GetString(data).Split(_commandSplit,
                StringSplitOptions.RemoveEmptyEntries);

            string[] scheme = commands[0].Split(' ');
            commands[0] = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(scheme[1]);
            request.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
            request.ProtocolVersion = new Version(1, 0);
            request.AllowAutoRedirect = false;
            request.Method = scheme[0];
            request.KeepAlive = false;
            request.Proxy = null;

            foreach (string command in commands)
            {
                if (string.IsNullOrWhiteSpace(command)) continue;

                string[] headerPair = command.Split(_headerPairSplit,
                    StringSplitOptions.RemoveEmptyEntries);

                if (headerPair.Length < 2)
                {
                    if (request.ContentLength == command.Length)
                        payload = Encoding.Default.GetBytes(command);
                    else if (request.Method == "POST" && request.ContentLength == request.RequestUri.Query.Length - 1)
                        payload = Encoding.Default.GetBytes(request.RequestUri.Query.Substring(1));

                    continue;
                }

                string header = headerPair[0],
                    value = command.GetChild(header + ": ");

                switch (header)
                {
                    case "Connection":
                    case "Keep-Alive":
                    case "Proxy-Connection": break;

                    case "Host": request.Host = value; break;
                    case "Accept": request.Accept = value; break;
                    case "Referer": request.Referer = value; break;
                    case "User-Agent": request.UserAgent = value; break;
                    case "Content-type": request.ContentType = value; break;
                    case "Content-Length": request.ContentLength = long.Parse(value); break;
                    case "If-Modified-Since": request.IfModifiedSince = DateTime.Parse(value); break;

                    default: request.Headers[header] = value; break;
                }
            }

            if (IsCacheDisabled)
                request.Headers["Cache-Control"] = "no-cache, no-store";

            return request;
        }
    }
}