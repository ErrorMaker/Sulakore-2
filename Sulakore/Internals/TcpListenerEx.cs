using System.Net;
using System.Net.Sockets;

namespace Sulakore
{
    internal sealed class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPAddress address, int port)
            : base(address, port)
        { }

        public new bool Active
        {
            get { return base.Active; }
        }
    }
}