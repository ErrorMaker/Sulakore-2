using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HostChangeClothesEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }
        public HGender Gender { get; private set; }
        public string FigureId { get; private set; }

        public HostChangeClothesEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            Gender = SKore.ToGender(_packet.ReadString(0));
            FigureId = _packet.ReadString(3);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, Gender: {1}, FigureId: {2}",
                Header, Gender, FigureId);
        }
    }
}