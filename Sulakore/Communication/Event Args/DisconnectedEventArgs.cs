using System;

namespace Sulakore.Communication
{
    public class DisconnectedEventArgs : EventArgs
    {
        public bool UnsubscribeFromEvents { get; set; }
    }
}