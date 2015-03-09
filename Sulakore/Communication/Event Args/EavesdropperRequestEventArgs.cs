using System.Net;
using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesdropperRequestEventArgs : CancelEventArgs
    {
        private readonly WebRequest _request;
        public WebRequest Request
        {
            get { return _request; }
        }

        private byte[] _payload;
        /// <summary>
        /// Gets or sets the request data being sent.
        /// </summary>
        public byte[] Payload
        {
            get { return _payload; }
            set
            {
                _payload = value;
                _request.ContentLength = _payload.Length;
            }
        }

        public EavesdropperRequestEventArgs(HttpWebRequest request)
        {
            _request = request;
        }
    }
}