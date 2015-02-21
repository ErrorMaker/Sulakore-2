using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostClickPlayerEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerId { get; private set; }
        public HPoint Tile { get; private set; }

        public HostClickPlayerEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerId = _packet.ReadInt(0);
            Tile = new HPoint(_packet.ReadInt(0), _packet.ReadInt(4));
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerId: {1}, Tile: {2}",
                Header, PlayerId, Tile);
        }
    }
}