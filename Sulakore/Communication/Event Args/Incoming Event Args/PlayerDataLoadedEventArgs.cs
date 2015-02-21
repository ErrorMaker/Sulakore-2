using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerDataLoadedEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;
        private readonly Dictionary<HGender, ReadOnlyCollection<IHPlayerData>> _groupByGender;
        private readonly Dictionary<string, ReadOnlyCollection<IHPlayerData>> _groupByFigureId;
        private readonly Dictionary<string, ReadOnlyCollection<IHPlayerData>> _groupByGroupName;

        public ushort Header { get; private set; }

        public ReadOnlyCollection<int> PlayerIds { get; private set; }
        public ReadOnlyCollection<string> FigureIds { get; private set; }
        public ReadOnlyCollection<string> GroupNames { get; private set; }
        public ReadOnlyCollection<string> PlayerNames { get; private set; }
        public ReadOnlyCollection<string> PlayerMottos { get; private set; }
        public ReadOnlyCollection<HPoint> TilesOccupied { get; private set; }
        public ReadOnlyCollection<IHPlayerData> LoadedPlayers { get; private set; }

        public PlayerDataLoadedEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            LoadedPlayers = new ReadOnlyCollection<IHPlayerData>(HPlayerData.Extract(_packet));

            var playerByGender = new Dictionary<HGender, List<IHPlayerData>>()
            {
                { HGender.Male, new List<IHPlayerData>() },
                { HGender.Female, new List<IHPlayerData>() },
                { HGender.Unknown, new List<IHPlayerData>() }
            };
            var playerByFigureId = new Dictionary<string, List<IHPlayerData>>();
            var playerByGroupName = new Dictionary<string, List<IHPlayerData>>();

            var figureIds = new List<string>();
            var groupNames = new List<string>();
            var playerMottos = new List<string>();
            var tilesOccupied = new List<HPoint>();
            var playerIds = new List<int>(LoadedPlayers.Count);
            var playerNames = new List<string>(LoadedPlayers.Count);

            foreach (IHPlayerData player in LoadedPlayers)
            {
                playerByGender[player.Gender].Add(player);

                if (playerByFigureId.ContainsKey(player.FigureId))
                    playerByFigureId[player.FigureId].Add(player);
                else playerByFigureId.Add(player.FigureId, new List<IHPlayerData>() { player });

                if (playerByGroupName.ContainsKey(player.FavoriteGroup))
                    playerByGroupName[player.FavoriteGroup].Add(player);
                else playerByGroupName.Add(player.FavoriteGroup, new List<IHPlayerData>() { player });

                if (!figureIds.Contains(player.FigureId))
                    figureIds.Add(player.FigureId);

                if (!groupNames.Contains(player.FavoriteGroup))
                    groupNames.Add(player.FavoriteGroup);

                if (!playerMottos.Contains(player.Motto))
                    playerMottos.Add(player.Motto);

                if (!tilesOccupied.Contains(player.Tile))
                    tilesOccupied.Add(player.Tile);

                playerIds.Add(player.PlayerId);
                playerNames.Add(player.PlayerName);
            }

            PlayerIds = new ReadOnlyCollection<int>(playerIds);
            FigureIds = new ReadOnlyCollection<string>(figureIds);
            GroupNames = new ReadOnlyCollection<string>(groupNames);
            PlayerNames = new ReadOnlyCollection<string>(playerNames);
            PlayerMottos = new ReadOnlyCollection<string>(playerMottos);
            TilesOccupied = new ReadOnlyCollection<HPoint>(tilesOccupied);

            _groupByGender = new Dictionary<HGender, ReadOnlyCollection<IHPlayerData>>();
            foreach (HGender key in playerByGender.Keys)
                _groupByGender[key] = new ReadOnlyCollection<IHPlayerData>(playerByGender[key]);

            _groupByFigureId = new Dictionary<string, ReadOnlyCollection<IHPlayerData>>();
            foreach (string key in playerByFigureId.Keys)
                _groupByFigureId[key] = new ReadOnlyCollection<IHPlayerData>(playerByFigureId[key]);

            _groupByGroupName = new Dictionary<string, ReadOnlyCollection<IHPlayerData>>();
            foreach (string key in playerByGroupName.Keys)
                _groupByGroupName[key] = new ReadOnlyCollection<IHPlayerData>(playerByGroupName[key]);
        }

        public ReadOnlyCollection<IHPlayerData> GroupByGender(HGender gender)
        {
            if (!_groupByGender.ContainsKey(gender)) return null;
            return _groupByGender[gender];
        }
        public ReadOnlyCollection<IHPlayerData> GroupByFigureId(string figureId)
        {
            if (!_groupByFigureId.ContainsKey(figureId)) return null;
            return _groupByFigureId[figureId];
        }
        public ReadOnlyCollection<IHPlayerData> GroupByGroupName(string groupName)
        {
            if (!_groupByGroupName.ContainsKey(groupName)) return null;
            return _groupByGroupName[groupName];
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Total Players Loaded: {1}",
                Header, LoadedPlayers.Count);
        }
    }
}