using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerDanceEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }

        public int PlayerIndex { get; private set; }
        public HDance Dance { get; private set; }

        public PlayerDanceEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerIndex = _packet.ReadInt(0);
            Dance = (HDance)_packet.ReadInt(4);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerIndex: {1}, Dance: {2}",
                Header, PlayerIndex, Dance);
        }
    }
}