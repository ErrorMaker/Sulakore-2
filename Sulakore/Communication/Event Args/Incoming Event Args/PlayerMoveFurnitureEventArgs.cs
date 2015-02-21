using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerMoveFurnitureEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int FurnitureId { get; private set; }
        public int FurnitureTypeId { get; private set; }
        public HPoint Tile { get; private set; }
        public HDirection Direction { get; private set; }
        public int FurnitureOwnerId { get; private set; }

        public PlayerMoveFurnitureEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            FurnitureId = _packet.ReadInt(0);
            FurnitureTypeId = _packet.ReadInt(4);
            Tile = new HPoint(_packet.ReadInt(8), _packet.ReadInt(12), _packet.ReadString(20));
            Direction = (HDirection)_packet.ReadInt(16);
            FurnitureOwnerId = _packet.ReadInt(_packet.Length - 6);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, FurnitureId: {1}, FurnitureTypeId: {2}, Tile: {3}, Direction: {4}, FurnitureOwnerId: {5}",
                Header, FurnitureId, FurnitureTypeId, Tile, Direction, FurnitureOwnerId);
        }
    }
}