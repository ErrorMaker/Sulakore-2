using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostGestureEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public HGesture Gesture { get; private set; }

        public HostGestureEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Gesture = (HGesture)_packet.ReadInt(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Gesture: {1}",
                Header, Gesture);
        }
    }
}