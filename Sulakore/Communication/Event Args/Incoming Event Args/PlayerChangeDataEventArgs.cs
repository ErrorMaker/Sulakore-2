using System;

using Sulakore.Habbo;
using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class PlayerChangeDataEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }

        public int PlayerIndex { get; private set; }
        public string FigureId { get; private set; }
        public HGender Gender { get; private set; }
        public string Motto { get; private set; }

        public PlayerChangeDataEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            int position = 4;
            PlayerIndex = _packet.ReadInt(0);
            FigureId = _packet.ReadString(ref position);
            Gender = SKore.ToGender(_packet.ReadString(position));
            Motto = _packet.ReadString(position);
        }

        public override string ToString()
        {
            return string.Format("Header: {0}, PlayerIndex: {1}, FigureId: {2}, Gender: {3}, Motto: {4}",
                Header, PlayerIndex, FigureId, Gender, Motto);
        }
    }
}