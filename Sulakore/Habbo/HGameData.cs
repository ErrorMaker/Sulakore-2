namespace Sulakore.Habbo
{
    public struct HGameData
    {
        public static readonly HGameData Empty;

        public bool IsEmpty
        {
            get { return Empty.Equals(this); }
        }

        private readonly string _host;
        public string Host
        {
            get { return _host; }
        }

        private readonly int _port;
        public int Port
        {
            get { return _port; }
        }

        private readonly string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
        }

        private readonly string _clientStarting;
        public string ClientStarting
        {
            get { return _clientStarting; }
        }

        private readonly string _userHash;
        public string UserHash
        {
            get { return _userHash; }
        }

        private readonly string _flashClientUrl;
        public string FlashClientUrl
        {
            get { return _flashClientUrl; }
        }

        private readonly string _flashClientBuild;
        public string FlashClientBuild
        {
            get { return _flashClientBuild; }
        }

        private readonly string _variables;
        public string Variables
        {
            get { return _variables; }
        }

        private readonly string _texts;
        public string Texts
        {
            get { return _texts; }
        }

        private readonly string _figurePartList;
        public string FigurePartList
        {
            get { return _figurePartList; }
        }

        private readonly string _overrideTexts;
        public string OverrideTexts
        {
            get { return _overrideTexts; }
        }

        private readonly string _overrideVariables;
        public string OverrideVariables
        {
            get { return _overrideVariables; }
        }

        private readonly string _productDataLoadUrl;
        public string ProductDataLoadUrl
        {
            get { return _productDataLoadUrl; }
        }

        private readonly string _furniDataLoadUrl;
        public string FurniDataLoadUrl
        {
            get { return _furniDataLoadUrl; }
        }

        public static bool operator ==(HGameData x, HGameData y)
        {
            return Equals(x, y);
        }
        public static bool operator !=(HGameData x, HGameData y)
        {
            return !(x == y);
        }

        public HGameData(string clientBody)
        {
            _host = clientBody.GetChild("\"connection.info.host\" : \"", '"');
            _port = int.Parse(clientBody.GetChild("\"connection.info.port\" : \"", '"').Split(',')[0]);

            _playerName = clientBody.GetChild("var habboName = \"", '"');
            _clientStarting = clientBody.GetChild("\"client.starting\" : \"", '"');

            _userHash = clientBody.GetChild("\"user.hash\" : \"", '"');

            _flashClientUrl = "http://" + clientBody.GetChild("\"flash.client.url\" : \"", '"').Substring(3) + "Habbo.swf";
            _flashClientBuild = _flashClientUrl.Split('/')[4];

            _variables = clientBody.GetChild("\"external.variables.txt\" : \"", '\"');
            _texts = clientBody.GetChild("\"external.texts.txt\" : \"", '\"');
            _figurePartList = clientBody.GetChild("\"external.figurepartlist.txt\" : \"", '"');
            _overrideTexts = clientBody.GetChild("\"external.override.texts.txt\" : \"", '"');
            _overrideVariables = clientBody.GetChild("\"external.override.variables.txt\" : \"", '"');
            _productDataLoadUrl = clientBody.GetChild("\"productdata.load.url\" : \"", '"');
            _furniDataLoadUrl = clientBody.GetChild("\"furnidata.load.url\" : \"", '"');
        }

        public bool Equals(HGameData other)
        {
            return string.Equals(_host, other._host)
                && int.Equals(_port, other._port)
                && string.Equals(_clientStarting, other._clientStarting)
                && string.Equals(_userHash, other._userHash)
                && string.Equals(_flashClientUrl, other._flashClientUrl)
                && string.Equals(_flashClientBuild, other._flashClientBuild)
                && string.Equals(_variables, other._variables)
                && string.Equals(_texts, other._texts)
                && string.Equals(_figurePartList, other._figurePartList)
                && string.Equals(_overrideTexts, other._overrideTexts)
                && string.Equals(_overrideVariables, other._overrideVariables)
                && string.Equals(_productDataLoadUrl, other._productDataLoadUrl)
                && string.Equals(_furniDataLoadUrl, other._furniDataLoadUrl);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (_host != null ? _host.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _port.GetHashCode();
                hashCode = (hashCode * 397) ^ (_clientStarting != null ? _clientStarting.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_userHash != null ? _userHash.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_flashClientUrl != null ? _flashClientUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_flashClientBuild != null ? _flashClientBuild.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_variables != null ? _variables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_texts != null ? _texts.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_figurePartList != null ? _figurePartList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_overrideTexts != null ? _overrideTexts.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_overrideVariables != null ? _overrideVariables.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_productDataLoadUrl != null ? _productDataLoadUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_furniDataLoadUrl != null ? _furniDataLoadUrl.GetHashCode() : 0);
                return hashCode;
            }
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HGameData && Equals((HGameData)obj);
        }
    }
}