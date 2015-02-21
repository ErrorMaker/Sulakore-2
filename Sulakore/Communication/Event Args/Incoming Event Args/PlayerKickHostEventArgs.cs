using System;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerKickHostEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }

        public PlayerKickHostEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;
        }

        public override string ToString()
        {
            return string.Format("Header: {0}", Header);
        }
    }
}