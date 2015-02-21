using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostKickPlayerEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerId { get; private set; }

        public HostKickPlayerEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerId = _packet.ReadInt(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerId: {1}",
                Header, PlayerId);
        }
    }
}