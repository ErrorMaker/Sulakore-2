using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostDanceEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public HDance Dance { get; private set; }

        public HostDanceEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Dance = (HDance)_packet.ReadInt(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Dance: {1}",
                Header, Dance);
        }
    }
}