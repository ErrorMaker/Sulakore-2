using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostChangeMottoEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public string Motto { get; private set; }

        public HostChangeMottoEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Motto = _packet.ReadString(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Motto: {1}",
                Header, Motto);
        }
    }
}