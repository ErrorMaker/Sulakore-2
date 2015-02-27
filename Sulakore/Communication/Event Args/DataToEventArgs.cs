using System.ComponentModel;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class DataToEventArgs : CancelEventArgs
    {
        private readonly HMessage _packet;
        /// <summary>
        /// Gets the packet that was intercepted.
        /// </summary>
        public HMessage Packet
        {
            get { return _packet; }
        }

        private readonly int _step;
        /// <summary>
        /// Gets the value that determines the order in-which the packet was intercepted.
        /// </summary>
        public int Step
        {
            get { return _step; }
        }

        private readonly bool _blocked;
        /// <summary>
        /// Gets or sets a value that indicates whether the packet should be sent.
        /// </summary>
        public bool Blocked
        {
            get { return _blocked; }
        }

        private HMessage _replacement;
        /// <summary>
        /// Gets or sets the packet that will replace the one originally intercepted.
        /// </summary>
        public HMessage Replacement
        {
            get { return _replacement; }
            set { _replacement = value; }
        }

        /// <summary>
        /// Gets a value that indicates whether the replacement data is different than its original data.
        /// </summary>
        public bool Replaced
        {
            get { return !_packet.ToString().Equals(Replacement.ToString()); }
        }

        public DataToEventArgs(byte[] data, HDestination destination, int step)
        {
            _step = step;
            _packet = new HMessage(data, destination);
            _replacement = new HMessage(data, destination);
        }
        public DataToEventArgs(byte[] data, HDestination destination, int step, HFilters filters)
            : this(data, destination, step)
        {
            _blocked = (destination == HDestination.Client)
               ? filters.InProcessFilters(ref _replacement)
               : filters.OutProcessFilters(ref _replacement);
        }
    }
}