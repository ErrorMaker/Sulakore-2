using System;
using System.Net;

namespace Sulakore.Communication
{
    public class EavesResponseEventArgs : EventArgs
    {
        public bool IsSwf { get; private set; }
        public string Url { get; private set; }
        public byte[] ResponeData { get; set; }
        public string Host { get; private set; }
        public string UserAgent { get; private set; }
        public string RawCookies { get; private set; }
        public CookieContainer Cookies { get; private set; }

        public EavesResponseEventArgs(byte[] responseData, string url, string host, bool isSwf, string userAgent, CookieCollection cookies, string rawCookies)
        {
            ResponeData = responseData;
            Url = url;
            Host = host;
            IsSwf = isSwf;
            UserAgent = userAgent;

            Cookies = new CookieContainer();
            Cookies.Add(cookies);
        }
    }
}