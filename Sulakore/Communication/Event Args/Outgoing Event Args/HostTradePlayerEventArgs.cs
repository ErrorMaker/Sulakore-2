using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostTradePlayerEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerIndex { get; private set; }

        public HostTradePlayerEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerIndex = _packet.ReadInt(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerIndex: {1}",
                Header, PlayerIndex);
        }
    }
}