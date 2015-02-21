using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostBanPlayerEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerId { get; private set; }
        public int RoomId { get; private set; }
        public HBan Ban { get; private set; }

        public HostBanPlayerEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerId = _packet.ReadInt(0);
            RoomId = _packet.ReadInt(4);
            Ban = SKore.ToBan(_packet.ReadString(8));
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerId: {1}, RoomId: {2}, Ban: {3}",
                Header, PlayerId, RoomId, Ban);
        }
    }
}