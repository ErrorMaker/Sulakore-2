using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;

using Sulakore.Protocol;
using Sulakore.Communication;
using Sulakore.Protocol.Encryption;
using System.Diagnostics;

namespace Sulakore.Habbo
{
    public class HSession : IHConnection, IDisposable
    {
        private string _clientUrl;
        private readonly Uri _httpHotelUri, _httpsHotelUri;

        public event EventHandler<EventArgs> Connected;
        protected virtual void OnConnected(EventArgs e)
        {
            EventHandler<EventArgs> handler = Connected;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<EventArgs> Disconnected;
        protected virtual void OnDisconnected(EventArgs e)
        {
            EventHandler<EventArgs> handler = Disconnected;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<DataToEventArgs> DataToClient;
        protected virtual void OnDataToClient(DataToEventArgs e)
        {
            EventHandler<DataToEventArgs> handler = DataToClient;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<DataToEventArgs> DataToServer;
        protected virtual void OnDataToServer(DataToEventArgs e)
        {
            EventHandler<DataToEventArgs> handler = DataToServer;
            if (handler != null) handler(this, e);
        }

        private readonly CookieContainer _cookies;
        public CookieContainer Cookies
        {
            get { return _cookies; }
        }

        public string Email { get; private set; }
        public HHotel Hotel { get; private set; }
        public string Password { get; private set; }

        public string Motto { get; private set; }
        public int PlayerId { get; private set; }
        public string FigureId { get; private set; }
        public string PlayerName { get; private set; }

        public string UniqueId { get; private set; }
        public DateTime MemberSince { get; private set; }

        public bool IsTrusted { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public bool IsProfileVisible { get; private set; }

        public string IdentityId { get; private set; }
        public string LoginLogId { get; private set; }
        public string SessionLogId { get; private set; }

        public string this[HPage page]
        {
            get { return GetResource(page); }
        }

        static HSession()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => { return true; };
        }
        public HSession(string email, string password, HHotel hotel)
        {
            Email = email;
            Hotel = hotel;
            Password = password;

            _cookies = new CookieContainer();
            _httpHotelUri = new Uri(Hotel.ToUrl(false));
            _httpsHotelUri = new Uri(Hotel.ToUrl(true));

            _inBuffer = new byte[1024];
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool Authenticate()
        {
            ExpireCookies();
            Cookies.SetCookies(_httpHotelUri, SKore.GetIpCookie(Hotel));
            try
            {
                byte[] postData = Encoding.UTF8.GetBytes(string.Format("{{\"email\":\"{0}\",\"password\":\"{1}\"}}", Email, Password));
                var loginRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/api/public/authentication/login", _httpsHotelUri.OriginalString));
                loginRequest.ContentType = "application/json;charset=UTF-8";
                loginRequest.UserAgent = SKore.ChromeAgent;
                loginRequest.AllowAutoRedirect = false;
                loginRequest.CookieContainer = Cookies;
                loginRequest.Method = "POST";
                loginRequest.Proxy = null;

                using (Stream requestStream = loginRequest.GetRequestStream())
                    requestStream.Write(postData, 0, postData.Length);

                using (var response = (HttpWebResponse)loginRequest.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    Cookies.Add(response.Cookies);
                    string responseBody = streamReader.ReadToEnd();
                    UpdateSession(responseBody);
                }
                return true;
            }
            catch { return false; }
        }
        public string GetResource(HPage page)
        {
            using (var webclientEx = new WebClientEx(Cookies))
            {
                string pageUrl = page.Juice(Hotel);
                webclientEx.Headers["User-Agent"] = SKore.ChromeAgent;
                if (page == HPage.Client)
                {
                    pageUrl = (_clientUrl ??
                        (_clientUrl = webclientEx.DownloadString(pageUrl).Split('"')[3]));

                    webclientEx.Proxy = null;
                }
                else if (page == HPage.Profile) pageUrl += PlayerName;

                return ProcessResource(webclientEx.DownloadString(pageUrl));
            }
        }

        private void ExpireCookies()
        {
            foreach (Cookie cookie in _cookies.GetCookies(_httpHotelUri))
                cookie.Expired = true;
        }
        private void UpdateSession(string response)
        {
            response = response.GetChild("{", '}')
                .Replace("\"", string.Empty);

            string[] playerInfo = response.Split(',');
            foreach (string playerAttribute in playerInfo)
            {
                string[] attribute = playerAttribute.Split(':');
                switch (attribute[0].ToLower())
                {
                    case "uniqueid": UniqueId = attribute[1]; break;
                    case "name": PlayerName = attribute[1]; break;
                    case "figurestring": FigureId = attribute[1]; break;
                    case "motto": Motto = attribute[1]; break;
                    case "membersince": MemberSince = DateTime.Parse(playerAttribute.GetChild(":")); break;
                    case "profilevisible": IsProfileVisible = bool.Parse(attribute[1]); break;
                    case "sessionlogid": SessionLogId = attribute[1]; break;
                    case "loginlogid": LoginLogId = attribute[1]; break;
                    case "identityid": IdentityId = attribute[1]; break;
                    case "emailverified": IsEmailVerified = bool.Parse(attribute[1]); break;
                    case "trusted": IsTrusted = bool.Parse(attribute[1]); break;
                    case "accountid": PlayerId = int.Parse(attribute[1]); break;
                }
            }
        }
        private string ProcessResource(string body)
        {
            return body;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            SKore.Unsubscribe(ref Connected);
            SKore.Unsubscribe(ref Disconnected);
            SKore.Unsubscribe(ref DataToClient);
            SKore.Unsubscribe(ref DataToServer);

            if (disposing)
            {
                // Managed
            }
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}",
                Email, Password, Hotel.ToDomain());
        }

        #region IHConnection Implementation
        private int _inStep;
        private byte[] _inCache;
        private readonly Socket _client;
        private readonly byte[] _inBuffer;

        Rc4 IHConnection.IncomingEncrypt
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        Rc4 IHConnection.OutgoingDecrypt
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        bool IHConnection.IsOutgoingEncrypted
        {
            get { return OutgoingEncrypt != null; }
        }

        private bool _shouldReceive;
        public bool ShouldReceive
        {
            get { return _shouldReceive; }
            set
            {
                if (_shouldReceive == value) return;

                if (_shouldReceive = value && IsConnected)
                    _client.BeginReceive(_inBuffer, 0, _inBuffer.Length, SocketFlags.None, OnReceiving, null);
            }
        }

        public int Port { get; private set; }
        public string Host { get; private set; }
        public HFilters Filters { get; private set; }
        public string[] Addresses { get; private set; }

        public bool IsConnected
        {
            get { return _client.Connected; }
        }
        public bool IsIncomingEncrypted { get; private set; }
        public bool IsOutgoingEncrypted { get; private set; }

        public Rc4 IncomingDecrypt { get; set; }
        public Rc4 OutgoingEncrypt { get; set; }

        public void Connect()
        {
            _shouldReceive = true;
            _client.BeginConnect(Addresses[0], Port, ClientConnected, null);
        }
        public void Disconnect()
        {
            if (_client.Connected)
            {
                _client.Shutdown(SocketShutdown.Both);
                _client.Close();
            }

            _inStep = 0;
            _inCache = null;
            _shouldReceive = false;

            IncomingDecrypt = OutgoingEncrypt = null;

            OnDisconnected(EventArgs.Empty);
        }

        public int SendToServer(byte[] data)
        {
            return 0;
        }
        public int SendToServer(ushort header, params object[] chunks)
        {
            return SendToServer(HMessage.Construct(header, chunks));
        }

        public int SendToClient(byte[] data)
        {
            return 0;
        }
        public int SendToClient(ushort header, params object[] chunks)
        {
            return SendToClient(HMessage.Construct(header, chunks));
        }

        private void OnReceiving(IAsyncResult ar)
        {
            try
            {
                SocketError errorCode;
                int length = _client.EndReceive(ar, out errorCode);
                if (errorCode != SocketError.Success) Disconnect();
                else if (ShouldReceive)
                {
                    var data = new byte[length];
                    Buffer.BlockCopy(_inBuffer, 0, data, 0, length);

                    if (IncomingDecrypt != null)
                        IncomingDecrypt.Parse(data);

                    if (_inStep == 2)
                    {
                        length = (data.Length >= 6 ? BigEndian.DecypherInt(data) : 0);
                        IsIncomingEncrypted = (length != data.Length - 4);
                    }

                    foreach (byte[] packet in ByteUtils.Split(ref _inCache, data, true))
                    { }

                    if (ShouldReceive) // 'ShouldReceive' could have been changed while processing the current data.
                        _client.BeginReceive(_inBuffer, 0, _inBuffer.Length, SocketFlags.None, OnReceiving, null);
                }
            }
            catch (ObjectDisposedException) { }
            catch { Disconnect(); }
        }
        private void ClientConnected(IAsyncResult ar)
        {
            _client.EndConnect(ar);
            OnConnected(EventArgs.Empty);

            if (ShouldReceive)
                _client.BeginReceive(_inBuffer, 0, _inBuffer.Length, SocketFlags.None, OnReceiving, null);
        }
        #endregion
    }
}