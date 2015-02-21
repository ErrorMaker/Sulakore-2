using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerDropFurnitureEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int FurnitureId { get; private set; }
        public int FurnitureTypeId { get; private set; }
        public HPoint Tile { get; set; }
        public HDirection Direction { get; set; }
        public bool IsRented { get; private set; }
        public int FurnitureOwnerId { get; private set; }
        public string FurnitureOwnerName { get; private set; }

        public PlayerDropFurnitureEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            int position = 0;
            FurnitureId = _packet.ReadInt(ref position);
            FurnitureTypeId = _packet.ReadInt(ref position);
            int x = _packet.ReadInt(ref position);
            int y = _packet.ReadInt(ref position);
            Direction = (HDirection)_packet.ReadInt(ref position);
            Tile = new HPoint(x, y, _packet.ReadString(ref position));
            _packet.ReadString(ref position);
            _packet.ReadInt(ref position);
            _packet.ReadInt(ref position);
            _packet.ReadString(ref position);
            IsRented = _packet.ReadInt(ref position) != 1;
            _packet.ReadInt(ref position);
            FurnitureOwnerId = _packet.ReadInt(ref position);
            FurnitureOwnerName = _packet.ReadString(ref position);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, FurnitureId: {1}, FurnitureTypeId: {2}, Tile: {3}, Direction: {4}, IsRented: {5}, FurnitureOwnerId: {6}, FurnitureOwnerName: {7}",
                Header, FurnitureId, FurnitureTypeId, Tile, Direction, IsRented, FurnitureOwnerId, FurnitureOwnerName);
        }
    }
}