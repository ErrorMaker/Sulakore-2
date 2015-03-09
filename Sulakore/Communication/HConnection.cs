using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Habbo.Headers;
using Sulakore.Protocol.Encryption;

namespace Sulakore.Communication
{
    public class HConnection : HTriggers, IHConnection, IDisposable
    {
        public event EventHandler<EventArgs> Connected;
        protected virtual void OnConnected(EventArgs e)
        {
            EventHandler<EventArgs> handler = Connected;

            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<DataToEventArgs> DataToClient;
        protected virtual void OnDataToClient(DataToEventArgs e)
        {
            EventHandler<DataToEventArgs> handler = DataToClient;

            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<DataToEventArgs> DataToServer;
        protected virtual void OnDataToServer(DataToEventArgs e)
        {
            EventHandler<DataToEventArgs> handler = DataToServer;

            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected;
        protected virtual void OnDisconnected(DisconnectedEventArgs e)
        {
            EventHandler<DisconnectedEventArgs> handler = Disconnected;

            if (handler != null)
                handler(this, e);
        }

        private TcpListenerEx _htcpExt;
        private Socket _clientS, _serverS;
        private int _toClientS, _toServerS, _socketCount;
        private byte[] _clientB, _serverB, _clientC, _serverC;
        private bool _hasOfficialSocket, _disconnectAllowed, _grabHeaders;

        private static readonly string _hostsPath;
        private readonly object _resetHostLock, _disconnectLock, _sendToClientLock, _sendToServerLock;

        public int Port { get; private set; }
        public string Host { get; private set; }
        public string[] Addresses { get; private set; }

        private readonly HFilters _filters;
        public HFilters Filters
        {
            get { return _filters; }
        }

        private Rc4 _serverDecrypt;
        public Rc4 ServerDecrypt
        {
            get { return _serverDecrypt; }
            set
            {
                if ((_serverDecrypt = value) != null)
                    ResponseEncrypted = false;
            }
        }
        public Rc4 ServerEncrypt { get; set; }

        private Rc4 _clientDecrypt;
        public Rc4 ClientDecrypt
        {
            get { return _clientDecrypt; }
            set
            {
                if ((_clientDecrypt = value) != null)
                    RequestEncrypted = false;
            }
        }
        public Rc4 ClientEncrypt { get; set; }

        public bool IsConnected
        {
            get { return _serverS != null && _serverS.Connected; }
        }
        public bool RequestEncrypted { get; private set; }
        public bool ResponseEncrypted { get; private set; }

        public int SocketSkip { get; set; }
        public string FlashClientBuild { get; private set; }

        static HConnection()
        {
            _hostsPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\drivers\\etc\\hosts";
        }
        public HConnection(string host, int port)
        {
            _filters = new HFilters();
            _resetHostLock = new object();
            _disconnectLock = new object();
            _sendToClientLock = new object();
            _sendToServerLock = new object();

            Host = host;
            Port = port;
            ResetHost();

            Addresses = Dns.GetHostAddresses(host)
                .Select(ip => ip.ToString()).ToArray();
        }

        public int SendToClient(byte[] data)
        {
            if (_clientS == null || !_clientS.Connected) return 0;
            lock (_sendToClientLock)
            {
                if (ServerEncrypt != null)
                    data = ServerEncrypt.SafeParse(data);

                return _clientS.Send(data);
            }
        }
        public int SendToClient(ushort header, params object[] chunks)
        {
            return SendToClient(HMessage.Construct(header, chunks));
        }

        public int SendToServer(byte[] data)
        {
            if (!IsConnected) return 0;

            lock (_sendToServerLock)
            {
                if (ClientEncrypt != null)
                    data = ClientEncrypt.SafeParse(data);

                return _serverS.Send(data);
            }
        }
        public int SendToServer(ushort header, params object[] chunks)
        {
            return SendToServer(HMessage.Construct(header, chunks));
        }

        public void ResetHost()
        {
            lock (_resetHostLock)
            {
                if (Host == null || !File.Exists(_hostsPath)) return;
                string[] hostsL = File.ReadAllLines(_hostsPath).Where(line => !line.Contains(Host) && !line.StartsWith("127.0.0.1")).ToArray();
                File.WriteAllLines(_hostsPath, hostsL);
            }
        }
        public void Disconnect()
        {
            if (!_disconnectAllowed) return;
            _disconnectAllowed = false;

            lock (_disconnectLock)
            {
                if (_clientS != null)
                {
                    _clientS.Shutdown(SocketShutdown.Both);
                    _clientS.Close();
                    _clientS = null;
                }
                if (_serverS != null)
                {
                    _serverS.Shutdown(SocketShutdown.Both);
                    _serverS.Close();
                    _serverS = null;
                }
                ResetHost();
                if (_htcpExt != null)
                {
                    _htcpExt.Stop();
                    _htcpExt = null;
                }
                _toClientS = _toServerS = _socketCount = 0;
                _clientB = _serverB = _clientC = _serverC = null;
                ClientEncrypt = ClientDecrypt = ServerEncrypt = ServerDecrypt = null;
                _hasOfficialSocket = _grabHeaders = RequestEncrypted = ResponseEncrypted = false;
                if (Disconnected != null)
                {
                    var disconnectedEventArgs = new DisconnectedEventArgs();
                    Disconnected(this, disconnectedEventArgs);

                    if (disconnectedEventArgs.UnsubscribeFromEvents)
                        Dispose(false);
                }
            }
        }
        public void Connect(bool loopback = false)
        {
            if (loopback)
            {
                if (!File.Exists(_hostsPath))
                    File.Create(_hostsPath).Close();
                File.SetAttributes(_hostsPath, FileAttributes.Normal);

                string[] hostsL = File.ReadAllLines(_hostsPath);
                if (!Array.Exists(hostsL, ip => Addresses.Contains(ip)))
                {
                    List<string> gameIPs = Addresses.ToList(); if (!gameIPs.Contains(Host)) gameIPs.Add(Host);
                    string mapping = string.Format("127.0.0.1\t\t{{0}}\t\t#{0}[{{1}}/{1}]", Host, gameIPs.Count);
                    File.AppendAllLines(_hostsPath, gameIPs.Select(ip => string.Format(mapping, ip, gameIPs.IndexOf(ip) + 1)));
                }
            }

            (_htcpExt = new TcpListenerEx(IPAddress.Any, Port)).Start();
            _htcpExt.BeginAcceptSocket(SocketAccepted, null);
            _disconnectAllowed = true;
        }

        private void SocketAccepted(IAsyncResult iAr)
        {
            try
            {
                if (++_socketCount == SocketSkip)
                {
                    _htcpExt.EndAcceptSocket(iAr).Close();
                    _htcpExt.BeginAcceptSocket(SocketAccepted, null);
                }
                else
                {
                    _clientS = _htcpExt.EndAcceptSocket(iAr);
                    _serverS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _serverS.BeginConnect(Addresses[0], Port, ConnectedToServer, null);
                }
            }
            catch
            {
                if (_htcpExt != null && _htcpExt.Active)
                {
                    if (_htcpExt.Pending()) _htcpExt.EndAcceptSocket(iAr).Close();
                    _htcpExt.BeginAcceptSocket(SocketAccepted, null);
                }
                else Disconnect();
            }
        }
        private void ConnectedToServer(IAsyncResult iAr)
        {
            _serverS.EndConnect(iAr);

            _grabHeaders = true;
            _serverB = new byte[1024];
            _clientB = new byte[512];
            ReadClientData();
            ReadServerData();
        }

        private void ReadClientData()
        {
            if (_clientS != null && _clientS.Connected)
                _clientS.BeginReceive(_clientB, 0, _clientB.Length, SocketFlags.None, DataFromClient, null);
        }
        private void DataFromClient(IAsyncResult iAr)
        {
            try
            {
                if (_clientS == null) return;
                int length = _clientS.EndReceive(iAr);
                if (length < 1) { Disconnect(); return; }

                byte[] data = new byte[length];
                Buffer.BlockCopy(_clientB, 0, data, 0, length);

                if (!_hasOfficialSocket)
                {
                    if (_hasOfficialSocket = (BigEndian.DecypherShort(data, 4) == 4000))
                    {
                        int buildLength = BigEndian.DecypherShort(data, 6);
                        FlashClientBuild = Encoding.Default.GetString(data, 8, buildLength);

                        ResetHost();
                        _htcpExt.Stop();
                        _htcpExt = null;
                        OnConnected(EventArgs.Empty);
                    }
                    else
                    {
                        SendToServer(data);
                        return;
                    }
                }

                if (ClientDecrypt != null)
                    ClientDecrypt.Parse(data);

                if (_toServerS == 3)
                {
                    int dLength = data.Length >= 6 ? BigEndian.DecypherInt(data) : 0;
                    RequestEncrypted = (dLength != data.Length - 4);
                }
                IList<byte[]> chunks = ByteUtils.Split(ref _clientC, data, !RequestEncrypted);

                foreach (byte[] chunk in chunks)
                    ProcessOutgoing(chunk);

                ReadClientData();
            }
            catch { Disconnect(); }
        }
        public void ProcessOutgoing(byte[] data)
        {
            ++_toServerS;
            if (_grabHeaders)
            {
                switch (_toServerS)
                {
                    case 2: Outgoing.InitiateHandshake = BigEndian.DecypherShort(data, 4); break;
                    case 3: Outgoing.ClientPublicKey = BigEndian.DecypherShort(data, 4); break;
                    case 4: Outgoing.FlashClientUrl = BigEndian.DecypherShort(data, 4); break;
                    case 6: Outgoing.ClientSsoTicket = BigEndian.DecypherShort(data, 4); break;
                    case 7: _grabHeaders = false; break;
                }
            }

            if (!RequestEncrypted)
                Task.Factory.StartNew(() => base.ProcessOutgoing(data), TaskCreationOptions.LongRunning)
                    .ContinueWith(OnException, TaskContinuationOptions.OnlyOnFaulted);

            if (DataToServer == null) SendToServer(data);
            else
            {
                var e = new DataToEventArgs(data, HDestination.Server, _toServerS, Filters);
                try { OnDataToServer(e); }
                catch { e.Cancel = true; }
                finally
                {
                    if (e.Cancel) SendToServer(e.Packet.ToBytes());
                    else if (!e.IsBlocked) SendToServer(e.Replacement.ToBytes());
                }
            }
        }

        private void ReadServerData()
        {
            if (IsConnected)
                _serverS.BeginReceive(_serverB, 0, _serverB.Length, SocketFlags.None, DataFromServer, null);
        }
        private void DataFromServer(IAsyncResult iAr)
        {
            try
            {
                if (_serverS == null) return;
                int length = _serverS.EndReceive(iAr);
                if (length < 1) { Disconnect(); return; }

                byte[] data = new byte[length];
                Buffer.BlockCopy(_serverB, 0, data, 0, length);

                if (!_hasOfficialSocket)
                {
                    SendToClient(data);
                    _htcpExt.BeginAcceptSocket(SocketAccepted, null);
                    return;
                }

                if (ServerDecrypt != null)
                    ServerDecrypt.Parse(data);

                if (_toClientS == 2)
                {
                    int dLength = data.Length >= 6 ? BigEndian.DecypherInt(data) : 0;
                    ResponseEncrypted = (dLength != data.Length - 4);
                }
                IList<byte[]> chunks = ByteUtils.Split(ref _serverC, data, !ResponseEncrypted);

                foreach (byte[] chunk in chunks)
                    ProcessIncoming(chunk);

                ReadServerData();
            }
            catch { Disconnect(); }
        }
        public void ProcessIncoming(byte[] data)
        {
            ++_toClientS;
            if (!ResponseEncrypted)
                Task.Factory.StartNew(() => base.ProcessIncoming(data), TaskCreationOptions.LongRunning)
                    .ContinueWith(OnException, TaskContinuationOptions.OnlyOnFaulted);

            if (DataToClient == null) SendToClient(data);
            else
            {
                var e = new DataToEventArgs(data, HDestination.Client, _toClientS, Filters);
                try { OnDataToClient(e); }
                catch { e.Cancel = true; }
                finally
                {
                    if (e.Cancel) SendToClient(e.Packet.ToBytes());
                    else if (!e.IsBlocked) SendToClient(e.Replacement.ToBytes());
                }
            }
        }

        private void OnException(Task task)
        {
            Debug.WriteLine(task.Exception.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            SKore.Unsubscribe(ref Connected);
            SKore.Unsubscribe(ref DataToClient);
            SKore.Unsubscribe(ref DataToServer);
            SKore.Unsubscribe(ref Disconnected);

            if (disposing)
            {
                Disconnect();

                Host = null;
                Addresses = null;
                Port = SocketSkip = 0;
                FlashClientBuild = string.Empty;
                CaptureEvents = LockEvents = false;
            }

            base.Dispose(disposing);
        }
    }
}