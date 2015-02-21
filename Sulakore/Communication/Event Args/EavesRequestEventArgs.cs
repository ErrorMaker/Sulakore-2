using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesRequestEventArgs : CancelEventArgs
    {
        public string Url { get; private set; }
        public string Host { get; private set; }

        public EavesRequestEventArgs(string url, string host)
        {
            Url = url;
            Host = host;
        }
    }
}