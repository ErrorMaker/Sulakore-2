using System;

using Sulakore.Protocol;
using Sulakore.Habbo;
using System.Collections.ObjectModel;

namespace Sulakore.Communication
{
    public class PlayerActionsDetectedEventArgs : EventArgs, IHabboEvent
    {
        private readonly HMessage _packet;

        public ushort Header { get; private set; }

        public ReadOnlyCollection<IHPlayerAction> PlayerActions { get; private set; }

        public PlayerActionsDetectedEventArgs(HMessage packet)
        {
            _packet = packet;
            Header = _packet.Header;

            PlayerActions = new ReadOnlyCollection<IHPlayerAction>(HPlayerAction.Extract(_packet));
        }
    }
}