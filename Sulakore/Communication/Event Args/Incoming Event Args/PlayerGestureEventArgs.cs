using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerGestureEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public int PlayerIndex { get; private set; }
        public HGesture Gesture { get; private set; }

        public PlayerGestureEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerIndex = _packet.ReadInt(0);
            Gesture = (HGesture)_packet.ReadInt(4);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerIndex: {1}, Gesture: {2}",
                Header, PlayerIndex, Gesture);
        }
    }
}