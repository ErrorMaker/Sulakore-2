using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostShoutEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public string Message { get; private set; }
        public HTheme Theme { get; private set; }

        public HostShoutEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Message = _packet.ReadString(0);
            Theme = (HTheme)_packet.ReadInt(_packet.Length - 6);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Message: {1}, Theme: {2}",
                Header, Message, Theme);
        }
    }
}