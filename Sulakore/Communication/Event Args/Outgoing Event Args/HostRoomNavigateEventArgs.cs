using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostRoomNavigateEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int RoomId { get; private set; }
        public string Passcode { get; private set; }

        public HostRoomNavigateEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            RoomId = _packet.ReadInt(0);
            Passcode = _packet.ReadString(4);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, RoomId: {1}, Passcode: {2}",
                Header, RoomId, Passcode);
        }
    }
}