using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostChangeStanceEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public HStance Stance { get; private set; }

        public HostChangeStanceEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Stance = (HStance)_packet.ReadInt(0);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Stance: {1}",
                Header, Stance);
        }
    }
}