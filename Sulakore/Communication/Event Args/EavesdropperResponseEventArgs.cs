using System.Net;
using System.ComponentModel;

namespace Sulakore.Communication
{
    public class EavesdropperResponseEventArgs : CancelEventArgs
    {
        private readonly WebResponse _response;
        public WebResponse Response
        {
            get { return _response; }
        }

        private byte[] _payload;
        /// <summary>
        /// Gets or sets the response data being received
        /// </summary>
        public byte[] Payload
        {
            get { return _payload; }
            set
            {
                _payload = value;
                _response.Headers["Content-Length"] = _payload.Length.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether Eavesdropper will terminate once this response has been processed.
        /// </summary>
        public bool ShouldTerminate { get; set; }

        public EavesdropperResponseEventArgs(WebResponse response)
        {
            _response = response;
        }
    }
}