using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class FurnitureDataLoadedEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;
        private readonly Dictionary<int, ReadOnlyCollection<IHFurnitureData>> _groupByOwnerId;
        private readonly Dictionary<string, ReadOnlyCollection<IHFurnitureData>> _groupByOwnerName;
        private readonly Dictionary<int, ReadOnlyCollection<IHFurnitureData>> _groupByFurnitureTypeId;

        public ushort Header { get; private set; }

        public ReadOnlyCollection<int> FurnitureIds { get; private set; }
        public ReadOnlyCollection<int> FurnitureOwnerIds { get; private set; }
        public ReadOnlyCollection<string> FurnitureOwnerNames { get; private set; }
        public ReadOnlyCollection<IHFurnitureData> LoadedFurniture { get; private set; }

        public FurnitureDataLoadedEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            LoadedFurniture = new ReadOnlyCollection<IHFurnitureData>(HFurnitureData.Extract(packet));

            var furnitureByTypeId = new Dictionary<int, List<IHFurnitureData>>();
            var furnitureByPlayerId = new Dictionary<int, List<IHFurnitureData>>();
            var furnitureByPlayerName = new Dictionary<string, List<IHFurnitureData>>();

            var furnitureIds = new List<int>();
            var furnitureOwnerIds = new List<int>();
            var furnitureOwnerNames = new List<string>();

            foreach (IHFurnitureData furniture in LoadedFurniture)
            {
                if (furnitureByTypeId.ContainsKey(furniture.FurnitureTypeId))
                    furnitureByTypeId[furniture.FurnitureTypeId].Add(furniture);
                else furnitureByTypeId.Add(furniture.FurnitureTypeId, new List<IHFurnitureData>() { furniture });

                if (furnitureByPlayerId.ContainsKey(furniture.FurnitureOwnerId))
                    furnitureByPlayerId[furniture.FurnitureOwnerId].Add(furniture);
                else furnitureByPlayerId.Add(furniture.FurnitureOwnerId, new List<IHFurnitureData>() { furniture });

                if (furnitureByPlayerName.ContainsKey(furniture.FurnitureOwnerName))
                    furnitureByPlayerName[furniture.FurnitureOwnerName].Add(furniture);
                else furnitureByPlayerName.Add(furniture.FurnitureOwnerName, new List<IHFurnitureData>() { furniture });

                furnitureIds.Add(furniture.FurnitureId);

                if (!furnitureOwnerIds.Contains(furniture.FurnitureOwnerId))
                    furnitureOwnerIds.Add(furniture.FurnitureOwnerId);

                if (!furnitureOwnerNames.Contains(furniture.FurnitureOwnerName))
                    furnitureOwnerNames.Add(furniture.FurnitureOwnerName);
            }

            FurnitureIds = new ReadOnlyCollection<int>(furnitureIds);
            FurnitureOwnerIds = new ReadOnlyCollection<int>(furnitureOwnerIds);
            FurnitureOwnerNames = new ReadOnlyCollection<string>(furnitureOwnerNames);

            _groupByFurnitureTypeId = new Dictionary<int, ReadOnlyCollection<IHFurnitureData>>();
            foreach (int key in furnitureByTypeId.Keys)
                _groupByFurnitureTypeId[key] = new ReadOnlyCollection<IHFurnitureData>(furnitureByTypeId[key]);

            _groupByOwnerId = new Dictionary<int, ReadOnlyCollection<IHFurnitureData>>();
            foreach (int key in furnitureByPlayerId.Keys)
                _groupByOwnerId[key] = new ReadOnlyCollection<IHFurnitureData>(furnitureByPlayerId[key]);

            _groupByOwnerName = new Dictionary<string, ReadOnlyCollection<IHFurnitureData>>();
            foreach (string key in furnitureByPlayerName.Keys)
                _groupByOwnerName[key] = new ReadOnlyCollection<IHFurnitureData>(furnitureByPlayerName[key]);
        }

        public ReadOnlyCollection<IHFurnitureData> GroupByOwnerId(int playerId)
        {
            if (!_groupByOwnerId.ContainsKey(playerId)) return null;
            return _groupByOwnerId[playerId];
        }
        public ReadOnlyCollection<IHFurnitureData> GroupByOwnerName(string playerName)
        {
            if (!_groupByOwnerName.ContainsKey(playerName)) return null;
            return _groupByOwnerName[playerName];
        }
        public ReadOnlyCollection<IHFurnitureData> GroupByFurnitureTypeId(int furnitureTypeId)
        {
            if (!_groupByFurnitureTypeId.ContainsKey(furnitureTypeId)) return null;
            return _groupByFurnitureTypeId[furnitureTypeId];
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Total Furniture Loaded: {1}",
                Header, LoadedFurniture.Count);
        }
    }
}