using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Habbo
{
    public class HFurnitureData : IHFurnitureData
    {
        public int FurnitureOwnerId { get; private set; }
        public string FurnitureOwnerName { get; private set; }

        public int FurnitureId { get; private set; }
        public int FurnitureIndex { get; private set; }
        public int FurnitureTypeId { get; private set; }

        public int State { get; private set; }
        public HPoint Tile { get; private set; }
        public HDirection Direction { get; private set; }

        public HFurnitureData(int furnitureOwnerId, string furnitureOwnerName, int furnitureId,
            int furnitureIndex, int furnitureTypeId, int state, HPoint tile, HDirection direction)
        {
            FurnitureOwnerId = furnitureOwnerId;
            FurnitureOwnerName = furnitureOwnerName;

            FurnitureId = furnitureId;
            FurnitureIndex = furnitureIndex;
            FurnitureTypeId = furnitureTypeId;

            State = state;
            Tile = tile;
            Direction = direction;
        }

        public static IList<IHFurnitureData> Extract(HMessage packet)
        {
            int furniOwnersCapacity, position = 0;
            var furniOwners = new Dictionary<int, string>(furniOwnersCapacity = packet.ReadInt(ref position));

            do furniOwners.Add(packet.ReadInt(ref position), packet.ReadString(ref position));
            while (furniOwners.Count < furniOwnersCapacity);

            string z;
            HDirection direction;
            int ownerId, furnitureId, furnitureTypeId, x, y, state;
            var furnitureDataList = new List<IHFurnitureData>(packet.ReadInt(ref position));
            do
            {
                z = string.Empty;
                direction = HDirection.North;
                ownerId = furnitureId = furnitureTypeId = x = y = state = 0;

                furnitureId = packet.ReadInt(ref position);
                furnitureTypeId = packet.ReadInt(ref position);

                x = packet.ReadInt(ref position);
                y = packet.ReadInt(ref position);
                direction = (HDirection)packet.ReadInt(ref position);
                z = packet.ReadString(ref position);
                packet.ReadString(ref position);
                packet.ReadInt(ref position);
                #region Parse Object Data
                switch (packet.ReadInt(ref position) & 0xFF)
                {
                    case 0: packet.ReadString(ref position); break;
                    case 1:
                    {
                        for (int i = packet.ReadInt(ref position); i > 0; i--)
                        {
                            packet.ReadString(ref position);
                            packet.ReadString(ref position);
                        }
                        break;
                    }
                    case 2:
                    {
                        for (int i = packet.ReadInt(ref position); i > 0; i--)
                            packet.ReadString(ref position);
                        break;
                    }
                    case 3:
                    {
                        packet.ReadString(ref position);
                        packet.ReadInt(ref position);
                        break;
                    }
                    case 5:
                    {
                        for (int i = packet.ReadInt(ref position); i > 0; i--)
                            packet.ReadInt(ref position);
                        break;
                    }
                    case 6:
                    {
                        packet.ReadString(ref position);
                        packet.ReadInt(ref position);
                        packet.ReadInt(ref position);
                        for (int i = packet.ReadInt(ref position); i > 0; i--)
                        {
                            packet.ReadInt(ref position);
                            for (int j = packet.ReadInt(ref position); j > 0; j--)
                                packet.ReadString(ref position);
                        }
                        break;
                    }
                    case 7:
                    {
                        packet.ReadString(ref position);
                        packet.ReadInt(ref position);
                        packet.ReadInt(ref position);
                        break;
                    }
                }
                #endregion
                packet.ReadInt(ref position);
                packet.ReadInt(ref position);
                ownerId = packet.ReadInt(ref position);
                if (furnitureTypeId < 0) packet.ReadString(ref position);

                furnitureDataList.Add(new HFurnitureData(ownerId, furniOwners[ownerId], furnitureId, furnitureDataList.Count, furnitureTypeId, state, new HPoint(x, y, z), direction));
            }
            while (furnitureDataList.Count < furnitureDataList.Capacity);
            return furnitureDataList;
        }

        public override string ToString()
        {
            return string.Format("FurnitureOwnerId: {0}, FurnitureOwnerName: {1}, FurnitureId: {2}, FurnitureIndex: {3}, FurnitureTypeId: {4}, Tile: {5}, Direction: {6}, State: {7}",
                FurnitureOwnerId, FurnitureOwnerName, FurnitureId, FurnitureIndex, FurnitureTypeId, Tile, Direction, State);
        }
    }
}