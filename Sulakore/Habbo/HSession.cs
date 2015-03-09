using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;

using Sulakore.Protocol;
using Sulakore.Communication;
using Sulakore.Protocol.Encryption;

namespace Sulakore.Habbo
{
    public sealed class HSession : IHConnection, IDisposable
    {
        #region Connection Events
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<DataToEventArgs> DataToServer;
        public event EventHandler<DataToEventArgs> DataToClient;
        public event EventHandler<DisconnectedEventArgs> Disconnected;
        #endregion

        #region Private Fields
        private Socket _serverS;
        private byte[] _serverB, _serverC;
        private int _toClientS, _toServerS;

        private readonly Encoding _encoding;
        private readonly object _disconnectLock;
        private readonly Uri _httpHotelUri, _httpsHotelUri;
        #endregion

        #region Public Properties
        public bool IsLoggedIn
        {
            get
            {
                using (var webClientEx = new WebClientEx(Cookies))
                {
                    webClientEx.Proxy = null;
                    webClientEx.Headers["User-Agent"] = SKore.ChromeAgent;
                    string body = webClientEx.DownloadString(_httpsHotelUri.OriginalString);
                    return body.Contains("window.location.replace('http:\\/\\/www.habbo." + Hotel.ToDomain() + "\\/me')");
                }
            }
        }

        private string _urlToken;
        public string UrlToken
        {
            get
            {
                if (!string.IsNullOrEmpty(_urlToken)) return _urlToken;
                LoadResource(HPage.Profile);
                return _urlToken;
            }
        }

        private string _csrfToken;
        public string CsrfToken
        {
            get
            {
                if (!string.IsNullOrEmpty(_csrfToken)) return _csrfToken;
                LoadResource(HPage.Profile);
                return _urlToken;
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get
            {
                if (!string.IsNullOrEmpty(_playerName)) return _playerName;
                LoadResource(HPage.Profile);
                return _playerName;
            }
        }

        private string _lastSignIn;
        public string LastSignIn
        {
            get
            {
                if (!string.IsNullOrEmpty(_lastSignIn)) return _lastSignIn;
                LoadResource(HPage.Me);
                return _lastSignIn;
            }
        }

        private string _createdOn;
        public string CreatedOn
        {
            get
            {
                if (!string.IsNullOrEmpty(_createdOn)) return _createdOn;
                LoadResource(HPage.Home);
                return _createdOn;
            }
        }

        private string _userHash;
        public string UserHash
        {
            get
            {
                if (!string.IsNullOrEmpty(_userHash)) return _userHash;
                LoadResource(HPage.Client);
                return _userHash;
            }
        }

        private string _motto;
        public string Motto
        {
            get
            {
                if (!string.IsNullOrEmpty(_motto)) return _motto;
                LoadResource(HPage.Me);
                return _motto;
            }
        }

        private bool? _homepageVisible;
        public bool HomepageVisible
        {
            get
            {
                if (_homepageVisible != null) return (bool)_homepageVisible;
                LoadResource(HPage.Profile);
                return (bool)_homepageVisible;
            }
        }

        private bool? _friendRequestAllowed;
        public bool FriendRequestAllowed
        {
            get
            {
                if (_friendRequestAllowed != null) return (bool)_friendRequestAllowed;
                LoadResource(HPage.Profile);
                return (bool)_friendRequestAllowed;
            }
        }

        private bool? _showOnlineStatus;
        public bool ShowOnlineStatus
        {
            get
            {
                if (_showOnlineStatus != null) return (bool)_showOnlineStatus;
                LoadResource(HPage.Profile);
                return (bool)_showOnlineStatus;
            }
        }

        private bool? _offlineMessaging;
        public bool OfflineMessaging
        {
            get
            {
                if (!_offlineMessaging != null) return (bool)_offlineMessaging;
                LoadResource(HPage.Profile);
                return (bool)_offlineMessaging;
            }
        }

        private bool? _friendsCanFollow;
        public bool FriendsCanFollow
        {
            get
            {
                if (_friendsCanFollow != null) return (bool)_friendsCanFollow;
                LoadResource(HPage.Profile);
                return (bool)_friendsCanFollow;
            }
        }

        private HGender _gender;
        public HGender Gender
        {
            get
            {
                if (_gender != HGender.Unknown) return _gender;
                LoadResource(HPage.Profile);
                return _gender;
            }
        }

        private int _age;
        public int Age
        {
            get
            {
                if (_age != 0) return _age;
                LoadResource(HPage.Profile);
                return _age;
            }
        }

        public string Email { get; private set; }
        public string Password { get; private set; }
        public HHotel Hotel { get; private set; }

        public int PlayerId { get; private set; }
        public string ClientStarting { get; set; }
        public bool EmailVerified { get; private set; }
        public CookieContainer Cookies { get; private set; }

        Rc4 IHConnection.ServerEncrypt { get; set; }
        public Rc4 ServerDecrypt { get; set; }

        public Rc4 ClientEncrypt { get; set; }
        Rc4 IHConnection.ClientDecrypt { get; set; }

        public HFilters Filters { get; private set; }

        private HGameData _gameData;
        public HGameData GameData
        {
            get
            {
                if (!_gameData.IsEmpty) return _gameData;
                LoadResource(HPage.Client);
                return _gameData;
            }
        }

        private string _flashClientUrl;
        public string FlashClientUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_flashClientUrl)) return _flashClientUrl;
                LoadResource(HPage.Client);
                return _flashClientUrl;
            }
        }

        private string _flashClientBuild;
        public string FlashClientBuild
        {
            get
            {
                if (!string.IsNullOrEmpty(_flashClientBuild)) return _flashClientBuild;
                LoadResource(HPage.Client);
                return _flashClientBuild;
            }
        }

        private int _port;
        public int Port
        {
            get
            {
                if (_port != 0) return _port;
                LoadResource(HPage.Client);
                return _port;
            }
        }

        private string _host;
        public string Host
        {
            get
            {
                if (!string.IsNullOrEmpty(_host)) return _host;
                LoadResource(HPage.Client);
                return _host;
            }
        }

        private string[] _addresses;
        public string[] Addresses
        {
            get
            {
                if (_addresses != null && _addresses.Length > 0) return _addresses;
                LoadResource(HPage.Client);
                return _addresses;
            }
        }

        private string _ssoTicket;
        public string SsoTicket
        {
            get
            {
                if (!string.IsNullOrEmpty(_ssoTicket)) return _ssoTicket;
                LoadResource(HPage.Client);
                return _ssoTicket;
            }
        }

        private bool _receiveData;
        public bool ReceiveData
        {
            get { return _receiveData; }
            set
            {
                if (!IsConnected) _receiveData = false;
                else if (_receiveData != value)
                {
                    bool wasReceiving = _receiveData;
                    _receiveData = value;
                    if (!wasReceiving) ReadServerData();
                }
            }
        }

        public bool IsConnected
        {
            get { return _serverS != null && _serverS.Connected; }
        }

        bool IHConnection.RequestEncrypted
        {
            get { return false; }
        }
        public bool ResponseEncrypted { get; private set; }
        #endregion

        static HSession()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => { return true; };
        }
        public HSession(string email, string password, HHotel hotel)
        {
            Email = email;
            Password = password;
            Hotel = hotel;

            _disconnectLock = new object();
            _encoding = new UTF8Encoding();

            _httpHotelUri = new Uri(Hotel.ToUrl());
            _httpsHotelUri = new Uri(Hotel.ToUrl(true));

            Cookies = new CookieContainer();
        }

        public string this[HPage page]
        {
            get { return LoadResource(page); }
        }

        public bool Login()
        {
            Dispose();
            Cookies.SetCookies(_httpHotelUri, SKore.GetIpCookie(Hotel));
            Cookies.SetCookies(_httpHotelUri, "cvid_token=1");

            try
            {
                byte[] postData = _encoding.GetBytes(string.Format("credentials.username={0}&credentials.password={1}", Email, Password));
                var loginRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/account/submit", _httpsHotelUri.OriginalString));
                loginRequest.ContentType = "application/x-www-form-urlencoded";
                loginRequest.UserAgent = SKore.ChromeAgent;
                loginRequest.AllowAutoRedirect = false;
                loginRequest.CookieContainer = Cookies;
                loginRequest.Method = "POST";
                loginRequest.Proxy = null;

                using (Stream dataStream = loginRequest.GetRequestStream())
                    dataStream.Write(postData, 0, postData.Length);

                string responseBody;
                using (var response = (HttpWebResponse)loginRequest.GetResponse())
                {
                    Cookies.Add(response.Cookies);
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        responseBody = streamReader.ReadToEnd();
                }

                if (responseBody.Contains("useOrCreateAvatar"))
                {
                    PlayerId = int.Parse(responseBody.GetChild("useOrCreateAvatar/", '?'));
                    var selectAvatarRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/identity/useOrCreateAvatar/{1}?next=", _httpHotelUri.OriginalString, PlayerId));
                    selectAvatarRequest.UserAgent = SKore.ChromeAgent;
                    selectAvatarRequest.CookieContainer = Cookies;
                    selectAvatarRequest.AllowAutoRedirect = false;
                    selectAvatarRequest.Method = "GET";
                    selectAvatarRequest.Proxy = null;

                    string redirectingTo;
                    using (var response = (HttpWebResponse)selectAvatarRequest.GetResponse())
                    {
                        Cookies.Add(response.Cookies);
                        redirectingTo = response.Headers["Location"];
                    }

                    var redirectRequest = (HttpWebRequest)WebRequest.Create(redirectingTo);
                    redirectRequest.UserAgent = SKore.ChromeAgent;
                    redirectRequest.CookieContainer = Cookies;
                    redirectRequest.AllowAutoRedirect = false;
                    redirectRequest.Method = "GET";
                    redirectRequest.Proxy = null;

                    using (var response = (HttpWebResponse)redirectRequest.GetResponse())
                    {
                        Cookies.Add(response.Cookies);

                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                            responseBody = streamReader.ReadToEnd();

                        if (redirectingTo.EndsWith("/me"))
                        {
                            EmailVerified = true;
                            HandleResource(HPage.Me, ref responseBody);
                            return true;
                        }
                    }

                    bool isTos = responseBody.Contains("/account/updateIdentityProfileTerms"),
                        isEmailVer = (responseBody.Contains("/account/updateIdentityProfileEmail"));
                    if (isTos || isEmailVer)
                    {
                        EmailVerified = !isEmailVer;

                        postData = _encoding.GetBytes(isTos ? "termsSelection=true" : "email=&skipEmailChange=true");
                        var termsOfServiceRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/account/updateIdentityProfile{1}",
                            _httpsHotelUri.OriginalString, isTos ? "Terms" : "Email"));

                        termsOfServiceRequest.ContentType = "application/x-www-form-urlencoded";
                        termsOfServiceRequest.Headers["Origin"] = _httpsHotelUri.OriginalString;
                        termsOfServiceRequest.UserAgent = SKore.ChromeAgent;
                        termsOfServiceRequest.AllowAutoRedirect = false;
                        termsOfServiceRequest.CookieContainer = Cookies;
                        termsOfServiceRequest.Referer = redirectingTo;
                        termsOfServiceRequest.Method = "POST";
                        termsOfServiceRequest.Proxy = null;

                        using (Stream dataStream = termsOfServiceRequest.GetRequestStream())
                            dataStream.Write(postData, 0, postData.Length);

                        using (var response = (HttpWebResponse)termsOfServiceRequest.GetResponse())
                            Cookies.Add(response.Cookies);

                        var reselectAvatarRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}/identity/useOrCreateAvatar/{1}?next=",
                            _httpHotelUri.OriginalString, PlayerId));

                        reselectAvatarRequest.UserAgent = SKore.ChromeAgent;
                        reselectAvatarRequest.CookieContainer = Cookies;
                        reselectAvatarRequest.AllowAutoRedirect = false;
                        reselectAvatarRequest.Method = "GET";
                        reselectAvatarRequest.Proxy = null;

                        using (var response = (HttpWebResponse)reselectAvatarRequest.GetResponse())
                        {
                            Cookies.Add(response.Cookies);
                            redirectingTo = response.Headers["Location"];
                        }

                        var homepageRedirectRequest = (HttpWebRequest)WebRequest.Create(redirectingTo);
                        homepageRedirectRequest.UserAgent = SKore.ChromeAgent;
                        homepageRedirectRequest.CookieContainer = Cookies;
                        homepageRedirectRequest.AllowAutoRedirect = false;
                        homepageRedirectRequest.Method = "GET";
                        homepageRedirectRequest.Proxy = null;

                        using (var response = (HttpWebResponse)homepageRedirectRequest.GetResponse())
                        {
                            Cookies.Add(response.Cookies);

                            using (var streamReader = new StreamReader(response.GetResponseStream()))
                                responseBody = streamReader.ReadToEnd();

                            bool willRedirectToHomepage = redirectingTo.EndsWith("/me");

                            if (willRedirectToHomepage)
                                HandleResource(HPage.Me, ref responseBody);

                            return willRedirectToHomepage;
                        }
                    }
                }
            }
            catch (WebException) { return false; }
            return false;
        }
        public Task<bool> LoginAsync()
        {
            return Task.Factory.StartNew(() => Login());
        }

        public void AddFriend(int playerId)
        {
            var formData = new NameValueCollection(1);
            formData.Add("accountId", playerId.ToString());

            DoXMLRequest(formData, _httpHotelUri.OriginalString + "/myhabbo/friends/add");
        }
        public Task AddFriendAsync(int playerId)
        {
            return Task.Factory.StartNew(() => AddFriend(playerId));
        }

        public void RemoveFriend(int playerId)
        {
            var formData = new NameValueCollection(2);
            formData.Add("friendId", playerId.ToString());
            formData.Add("pageSize", "30");

            DoXMLRequest(formData, _httpsHotelUri.OriginalString + "/friendmanagement/ajax/deletefriends");

        }
        public Task RemoveFriendAsync(int playerId)
        {
            return Task.Factory.StartNew(() => RemoveFriend(playerId));
        }

        public void UpdateProfile(string motto, bool homepageVisible, bool friendRequestAllowed, bool showOnlineStatus, bool offlineMessaging, bool friendsCanFollow)
        {
            var formData = new NameValueCollection(9);
            formData.Add("__app_key", CsrfToken);
            formData.Add("urlToken", UrlToken);
            formData.Add("tab", "2");
            formData.Add("motto", motto);
            formData.Add("visibility", homepageVisible ? "EVERYONE" : "NOBODY");
            formData.Add("friendRequestsAllowed", friendRequestAllowed.ToString().ToLower());
            formData.Add("showOnlineStatus", showOnlineStatus.ToString().ToLower());
            formData.Add("persistentMessagingAllowed", offlineMessaging.ToString().ToLower());
            formData.Add("followFriendMode", Convert.ToByte(friendsCanFollow).ToString());

            DoXMLRequest(formData, _httpsHotelUri.OriginalString + "/profile/profileupdate");
        }
        public Task UpdateProfileAsync(string motto, bool homepageVisible, bool friendRequestAllowed, bool showOnlineStatus, bool offlineMessaging, bool friendsCanFollow)
        {
            return Task.Factory.StartNew(() => UpdateProfile(motto, homepageVisible, friendRequestAllowed, showOnlineStatus, offlineMessaging, friendsCanFollow));
        }

        public string RenewTicket()
        {
            LoadResource(HPage.Client);
            return _ssoTicket;
        }
        public Task<string> RenewTicketAsync()
        {
            return Task.Factory.StartNew(() => RenewTicket());
        }

        public void DownloadClient(string path)
        {
            using (var webClientEx = new WebClientEx(Cookies))
            {
                webClientEx.Proxy = null;
                webClientEx.Headers["User-Agent"] = SKore.ChromeAgent;
                webClientEx.DownloadFile(FlashClientUrl, path);
            }
        }
        public Task DownloadClientAsync(string path)
        {
            return Task.Factory.StartNew(() => DownloadClient(path));
        }

        public void Connect()
        {
            _serverS = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverS.BeginConnect(Addresses[0], Port, ConnectedToServer, null);
        }
        public void Disconnect(bool dispose = true)
        {
            lock (_disconnectLock)
            {
                if (_serverS == null) return;

                _serverS.Shutdown(SocketShutdown.Both);
                _serverS.Dispose();
                _serverS = null;

                _receiveData = false;
                _serverB = _serverC = null;
                _toClientS = _toServerS = 0;

                if (Disconnected != null && !dispose)
                {
                    var disconnectedEventArgs = new DisconnectedEventArgs();
                    Disconnected(this, disconnectedEventArgs);
                    dispose = disconnectedEventArgs.UnsubscribeFromEvents;
                }

                if (dispose)
                {
                    SKore.Unsubscribe(ref Connected);
                    SKore.Unsubscribe(ref DataToClient);
                    SKore.Unsubscribe(ref DataToServer);
                    SKore.Unsubscribe(ref Disconnected);
                }
            }
        }

        public int SendToServer(ushort header, params object[] chunks)
        {
            return SendToServer(HMessage.Construct(header, chunks));
        }
        public Task<int> SendToServerAsync(ushort header, params object[] chunks)
        {
            return Task.Factory.StartNew(() => SendToServer(header, chunks));
        }

        public int SendToServer(byte[] data)
        {
            if (IsConnected)
            {
                if (DataToServer != null)
                {
                    try { DataToServer(this, new DataToEventArgs(data, HDestination.Server, ++_toServerS)); }
                    catch { }
                }

                if (ClientEncrypt != null)
                    data = ClientEncrypt.SafeParse(data);

                try { _serverS.Send(data); }
                catch { Disconnect(); return 0; }

                return data.Length;
            }
            return 0;
        }
        public Task<int> SendToServerAsync(byte[] data)
        {
            return Task.Factory.StartNew(() => SendToServer(data));
        }

        int IHConnection.SendToClient(byte[] data)
        { return 0; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Email != null ? Email.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Hotel.GetHashCode();
                return hashCode;
            }
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HSession && Equals((HSession)obj);
        }
        public bool Equals(HSession other)
        {
            return string.Equals(Email, other.Email) && Enum.Equals(Hotel, other.Hotel);
        }

        public void Refresh()
        {
            _urlToken = string.Empty;
            _csrfToken = string.Empty;
            _playerName = string.Empty;
            _lastSignIn = string.Empty;
            _createdOn = string.Empty;
            _userHash = string.Empty;
            _motto = string.Empty;
            _homepageVisible = false;
            _friendRequestAllowed = false;
            _showOnlineStatus = false;
            _offlineMessaging = false;
            _friendsCanFollow = false;
            _gender = HGender.Unknown;
            _age = 0;
            ClientStarting = string.Empty;
            _gameData = HGameData.Empty;
            _flashClientBuild = string.Empty;
            _flashClientUrl = string.Empty;
            _port = 0;
            _host = string.Empty;
            _addresses = null;
            _ssoTicket = string.Empty;
        }
        public void Dispose()
        {
            var cookies = Cookies.GetCookies(_httpHotelUri);
            foreach (Cookie cookie in cookies) cookie.Expired = true;

            Refresh();
            Disconnect();
        }

        private void LogStep(string step)
        {
            var formData = new NameValueCollection(1);
            formData.Add("step", step);

            DoXMLRequest(formData, _httpsHotelUri.OriginalString + "/new-user-reception/log-step");
        }
        private void ClientlogUpdate(string flashStep)
        {
            var formData = new NameValueCollection(1);
            formData.Add("flashStep", flashStep);

            DoXMLRequest(formData, _httpsHotelUri.OriginalString + "/clientlog/update");
        }
        private void DoXMLRequest(NameValueCollection formData, string address)
        {
            using (var webClientEx = new WebClientEx(Cookies))
            {
                webClientEx.Proxy = null;
                webClientEx.Headers["X-App-Key"] = CsrfToken;
                webClientEx.Headers["User-Agent"] = SKore.ChromeAgent;
                webClientEx.Headers["Referer"] = _httpsHotelUri.OriginalString + "/client";

                if (formData != null) webClientEx.UploadValues(address, "POST", formData);
                else webClientEx.DownloadString(address);
            }
        }

        private string LoadResource(HPage page)
        {
            using (var webClientEx = new WebClientEx(Cookies))
            {
                if (page != HPage.Client)
                    webClientEx.Proxy = null;

                string url = page.Juice(Hotel) + (page == HPage.Home ? PlayerName : string.Empty);
                webClientEx.Headers["User-Agent"] = SKore.ChromeAgent;
                string body = webClientEx.DownloadString(url);
                HandleResource(page, ref body);
                return body;
            }
        }
        private void HandleResource(HPage page, ref string body)
        {
            PlayerId = int.Parse(body.GetChild("var habboId = ", ';'));
            _playerName = body.GetChild("var habboName = \"", '\"');
            _age = int.Parse(body.GetChild("kvage=", ';'));
            _gender = (HGender)Char.ToUpper(body.GetChild("kvgender=", ';')[0]);
            _csrfToken = body.GetChild("<meta name=\"csrf-token\" content=\"", '\"');

            switch (page)
            {
                case HPage.Me:
                {
                    string[] infoBoxes = body.GetChilds("<div class=\"content\">", '<', false);
                    _motto = infoBoxes[6].Split('>')[1];
                    _lastSignIn = infoBoxes[12].Split('>')[1];
                    break;
                }
                case HPage.Home:
                {
                    _createdOn = body.GetChild("<div class=\"birthday date\">", '<');
                    _motto = body.GetChild("<div class=\"profile-motto\">", '<');
                    break;
                }
                case HPage.Profile:
                {
                    _urlToken = body.GetChild("name=\"urlToken\" value=\"", '\"');

                    _homepageVisible = body.GetChild("name=\"visibility\" value=\"EVERYONE\"", '/').Contains("checked");
                    _friendRequestAllowed = body.GetChild("name=\"friendRequestsAllowed\"", '/').Contains("checked");
                    _showOnlineStatus = body.GetChild("name=\"showOnlineStatus\" value=\"true\"", '/').Contains("checked");
                    _offlineMessaging = body.GetChild("name=\"persistentMessagingAllowed\" checked=\"checked\"", '/').Contains("true");
                    _friendsCanFollow = body.GetChild("name=\"followFriendMode\" value=\"1\"").Contains("checked");
                    break;
                }
                case HPage.Client:
                {
                    _gameData = new HGameData(body);

                    _host = _gameData.Host;
                    _port = _gameData.Port;
                    _userHash = _gameData.UserHash;
                    _flashClientUrl = _gameData.FlashClientUrl;
                    _flashClientBuild = _gameData.FlashClientBuild;

                    _addresses = Dns.GetHostAddresses(_host).Select(ip => ip.ToString()).ToArray();
                    _ssoTicket = body.GetChild("\"sso.ticket\" : \"", '\"');

                    if (string.IsNullOrEmpty(ClientStarting)) ClientStarting = _gameData.ClientStarting;
                    else body = body.Replace(_gameData.ClientStarting, ClientStarting);

                    body = body.Replace("\"\\//", "\"http://");
                    break;
                }
            }
        }

        private void ConnectedToServer(IAsyncResult iAr)
        {
            _serverS.EndConnect(iAr);

            _serverB = new byte[1024];

            _receiveData = true;
            ReadServerData();

            if (Connected != null)
                Connected(this, EventArgs.Empty);
        }
        private void ReadServerData()
        {
            if (IsConnected && ReceiveData)
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

                if (ServerDecrypt != null)
                    ServerDecrypt.Parse(data);

                if (_toClientS == 2)
                {
                    int dLength = data.Length >= 6 ? BigEndian.DecypherInt(data) : 0;
                    ResponseEncrypted = (dLength != data.Length - 4);
                }
                IList<byte[]> chunks = ByteUtils.Split(ref _serverC, data, ResponseEncrypted);

                foreach (byte[] chunk in chunks)
                {
                    ++_toClientS;

                    if (DataToClient != null)
                        DataToClient(this, new DataToEventArgs(chunk, HDestination.Client, _toClientS));
                }
                ReadServerData();
            }
            catch { Disconnect(); }
        }

        public static IList<HSession> Extract(string path, char delimiter = ':')
        {
            var accounts = new List<HSession>();
            using (var streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line.Contains(delimiter))
                    {
                        string[] credentials = line.Split(delimiter);
                        if (credentials.Count(x => !string.IsNullOrEmpty(x)) != 3) break;
                        accounts.Add(new HSession(credentials[0], credentials[1], SKore.ToHotel(credentials[2])));
                        continue;
                    }
                    if (line.Contains('@') && !streamReader.EndOfStream)
                    {
                        string email = line;
                        string password = streamReader.ReadLine();
                        if (!streamReader.EndOfStream)
                        {
                            HHotel hotel = SKore.ToHotel((streamReader.ReadLine()).GetChild(" / "));
                            accounts.Add(new HSession(email, password, hotel));
                        }
                        else return accounts.ToArray();
                    }
                }
            }
            return accounts;
        }
        public static Task<IList<HSession>> ExtractAsync(string path, char delimiter = ':')
        {
            return Task.Factory.StartNew(() => Extract(path, delimiter));
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Email, Password, Hotel.ToDomain());
        }
    }
}