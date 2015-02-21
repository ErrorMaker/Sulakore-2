using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostMutePlayerEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerId { get; private set; }
        public int RoomId { get; private set; }
        public int Minutes { get; private set; }

        public HostMutePlayerEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerId = _packet.ReadInt(0);
            RoomId = _packet.ReadInt(4);
            Minutes = _packet.ReadInt(8);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerId: {1}, RoomId: {2}, Minutes: {3}",
                Header, PlayerId, RoomId, Minutes);
        }
    }
}