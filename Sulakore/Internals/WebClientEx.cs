using System;
using System.Net;

namespace Sulakore
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal sealed class WebClientEx : WebClient
    {
        public CookieContainer Cookies { get; set; }

        public WebClientEx()
        { }
        public WebClientEx(CookieContainer cookies)
        {
            Cookies = cookies;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            var httpRequest = (request as HttpWebRequest);
            if (httpRequest == null) return request;

            if (Cookies != null)
                httpRequest.CookieContainer = Cookies;

            return httpRequest;
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);

            var httpResponse = (response as HttpWebResponse);
            if (httpResponse == null) return response;

            if (Cookies != null)
                Cookies.Add(httpResponse.Cookies);

            return response;
        }
    }
}