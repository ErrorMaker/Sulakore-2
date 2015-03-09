using System;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostSendMessageEventArgs : EventArgs, IHabboEvent
    {
        public readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerId { get; private set; }
        public string Message { get; private set; }

        public HostSendMessageEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerId = _packet.ReadInt(0);
            Message = _packet.ReadString(4);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerId: {1}, Message: {2}",
                Header, PlayerId, Message);
        }
    }
}