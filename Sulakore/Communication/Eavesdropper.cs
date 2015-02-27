using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using System.Diagnostics;

namespace Sulakore.Communication
{
    public static class Eavesdropper
    {
        #region Private Fields
        private static bool _isRunning;

        private static readonly HttpListener _httpListener;
        private static readonly List<IAsyncResult> _processingCallbacks;
        #endregion

        #region Eavesdropper Events
        public static event EventHandler<EavesRequestEventArgs> OnEavesRequest;
        public static event EventHandler<EavesResponseEventArgs> OnEavesResponse;
        #endregion

        #region Public Properties
        public static bool DisableCache { get; set; }
        #endregion

        #region Constructor(s)
        static Eavesdropper()
        {
            _httpListener = new HttpListener();
            _processingCallbacks = new List<IAsyncResult>();
        }
        #endregion

        #region Public Methods
        public static void Initiate(int port)
        {
            Terminate();

            _isRunning = true;
            _httpListener.Prefixes.Clear();
            _httpListener.Prefixes.Add("http://*:" + port + "/");
            _httpListener.Start();

            NativeMethods.EnableProxy(port);
            Task.Factory.StartNew(CaptureClients, TaskCreationOptions.LongRunning);
        }
        public static void Terminate()
        {
            _isRunning = false;
            if (_processingCallbacks.Count == 0)
            {
                NativeMethods.DisableProxy();
                if (_httpListener.IsListening)
                    _httpListener.Stop();
            }
        }
        #endregion

        #region Private Methods
        private static void CaptureClients()
        {
            if (_isRunning)
                _httpListener.BeginGetContext(RequestReceived, null);
        }
        private static void RequestReceived(IAsyncResult ar)
        {
            CaptureClients();
            try
            {
                HttpListenerContext context = _httpListener.EndGetContext(ar);
                _processingCallbacks.Add(ar);

                var request = (HttpWebRequest)ConstructRequest(context.Request);
                if (OnEavesRequest != null)
                {
                    var requestArgs = new EavesRequestEventArgs(request.RequestUri.AbsoluteUri, request.Host);
                    OnEavesRequest(context, requestArgs);
                    if (requestArgs.Cancel) context.Response.Close();
                }
                if (request.ContentLength > 0 || request.SendChunked)
                {
                    var requestData = new byte[request.ContentLength > 0 ? request.ContentLength : 8192];
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        int length;
                        while ((length = context.Request.InputStream.Read(requestData, 0, requestData.Length)) > 0)
                            requestStream.Write(requestData, 0, requestData.Length);
                    }
                }

                #region HttpWebResponse Constructer
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    #region Safely Populate Headers
                    var responseHeaders = response.Headers;
                    foreach (string header in responseHeaders.Keys)
                    {
                        string value = responseHeaders[header];
                        switch (header)
                        {
                            case "Transfer-Encoding": context.Response.SendChunked = false; break;
                            case "Content-Length": context.Response.ContentLength64 = long.Parse(value); break;

                            default: context.Response.Headers[header] = value; break;
                        }
                    }

                    if (DisableCache)
                        context.Response.Headers["Cache-Control"] = "no-cache, no store";
                    #endregion

                    var responseData = new byte[0];
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        int length;
                        byte[] block;
                        var chunk = new byte[response.ContentLength > 0 ? response.ContentLength : 8192];

                        while ((length = responseStream.Read(chunk, 0, chunk.Length)) > 0)
                        {
                            block = ByteUtils.CopyBlock(chunk, 0, length);
                            responseData = ByteUtils.Merge(responseData, block);
                        }
                    }

                    if (OnEavesResponse != null)
                    {
                        var arguments = new EavesResponseEventArgs(responseData,
                            response.ResponseUri.AbsoluteUri,
                            response.ResponseUri.Host,
                            context.Response.ContentType == "application/x-shockwave-flash",
                            request.UserAgent,
                            response.Cookies,
                            response.Headers["Set-Cookie"]);

                        OnEavesResponse(context, arguments);
                        responseData = arguments.ResponeData;
                    }

                    context.Response.ContentLength64 = responseData.Length;
                    context.Response.OutputStream.Write(responseData, 0, responseData.Length);
                }
                #endregion
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
            finally
            {
                if (_processingCallbacks.Contains(ar))
                    _processingCallbacks.Remove(ar);

                if (!_isRunning) Terminate();
            }
        }

        private static WebRequest ConstructRequest(HttpListenerRequest clientRequest)
        {
            var request = (HttpWebRequest)WebRequest.Create(clientRequest.RawUrl);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ProtocolVersion = clientRequest.ProtocolVersion;
            request.Method = clientRequest.HttpMethod;
            request.AllowAutoRedirect = false;
            request.KeepAlive = false;
            request.Proxy = null;

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(clientRequest.Url, clientRequest.Cookies);

            #region Safely Populate Headers
            var requestHeaders = clientRequest.Headers;
            foreach (string header in requestHeaders.Keys)
            {
                string value = requestHeaders[header];
                switch (header)
                {
                    case "Connection":
                    case "Keep-Alive":
                    case "Proxy-Connection": break;

                    case "Range":
                    {
                        string[] ranges = value.GetChilds("bytes=", '-', false);
                        request.AddRange(long.Parse(ranges[0]));
                        if (ranges.Length > 1) request.AddRange(long.Parse(ranges[1]));
                        break;
                    }
                    case "Host": request.Host = value; break;
                    case "Accept": request.Accept = value; break;
                    case "Referer": request.Referer = value; break;
                    case "User-Agent": request.UserAgent = value; break;
                    case "Content-Type": request.ContentType = value; break;
                    case "Content-Length": request.ContentLength = long.Parse(value); break;
                    case "If-Modified-Since": request.IfModifiedSince = DateTime.Parse(value); break;

                    default: request.Headers[header] = value; break;
                }
            }

            if (DisableCache)
                request.Headers["Cache-Control"] = "no-cache, no store";
            #endregion

            return request;
        }
        #endregion
    }
}