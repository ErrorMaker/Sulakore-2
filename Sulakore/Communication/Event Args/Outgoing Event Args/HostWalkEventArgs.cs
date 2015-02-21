using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostWalkEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public HPoint Tile { get; private set; }

        public HostWalkEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Tile = new HPoint(_packet.ReadInt(0), _packet.ReadInt(4));
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Tile: {1}",
                Header, Tile);
        }
    }
}