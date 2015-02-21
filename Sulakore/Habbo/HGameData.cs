namespace Sulakore.Habbo
{
    public struct HGameData
    {
        public static readonly HGameData Empty;

        public bool IsEmpty
        {
            get { return Empty.Equals(this); }
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

        public HGameData(string variables, string texts, string figurePartList, string overrideTexts,
            string overrideVariables, string productDataLoadUrl, string furniDataLoadUrl)
        {
            _variables = variables;
            _texts = texts;
            _figurePartList = figurePartList;
            _overrideTexts = overrideTexts;
            _overrideVariables = overrideVariables;
            _productDataLoadUrl = productDataLoadUrl;
            _furniDataLoadUrl = furniDataLoadUrl;
        }

        public bool Equals(HGameData other)
        {
            return string.Equals(_variables, other._variables)
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
                int hashCode = (_variables != null ? _variables.GetHashCode() : 0);
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

        public static HGameData Parse(string clientBody)
        {
            string variables = clientBody.GetChild("\"external.variables.txt\" : \"", '\"');
            string texts = clientBody.GetChild("\"external.texts.txt\" : \"", '\"');
            string figurePartList = clientBody.GetChild("\"external.figurepartlist.txt\" : \"", '\"');
            string overrideTexts = clientBody.GetChild("\"external.override.texts.txt\" : \"", '\"');
            string overrideVariables = clientBody.GetChild("\"external.override.variables.txt\" : \"", '\"');
            string productDataLoadUrl = clientBody.GetChild("\"productdata.load.url\" : \"", '\"');
            string furniDataLoadUrl = clientBody.GetChild("\"furnidata.load.url\" : \"", '\"');
            return new HGameData(variables, texts, figurePartList, overrideTexts, overrideVariables, productDataLoadUrl, furniDataLoadUrl);
        }
    }
}