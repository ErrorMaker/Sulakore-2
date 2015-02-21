using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostMoveFurnitureEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int FurnitureId { get; private set; }
        public HPoint Tile { get; private set; }
        public HDirection Direction { get; private set; }

        public HostMoveFurnitureEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            FurnitureId = _packet.ReadInt(0);
            Tile = new HPoint(_packet.ReadInt(4), _packet.ReadInt(8));
            Direction = (HDirection)_packet.ReadInt(12);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, FurnitureId: {1}, Tile: {2}, Direction: {3}",
                Header, FurnitureId, Tile, Direction);
        }
    }
}