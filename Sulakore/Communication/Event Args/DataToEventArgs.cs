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

        private readonly int _repeat;
        /// <summary>
        /// Gets the amount of times the packet will be re-sent.
        /// </summary>
        public int Repeat
        {
            get { return _repeat; }
        }

        private readonly bool _blocked;
        /// <summary>
        /// Gets or sets a value that indicates whether the packet should be sent.
        /// </summary>
        public bool Blocked
        {
            get { return _blocked; }
        }

        /// <summary>
        /// Gets or sets the packet that will replace the one originally intercepted.
        /// </summary>
        public HMessage Replacement { get; set; }

        public DataToEventArgs(byte[] data, HDestination destination, int step)
        {
            _step = step;
            _packet = new HMessage(data, destination);
            Replacement = new HMessage(data, destination);
        }
        public DataToEventArgs(byte[] data, HDestination destination, int step, HFilters filters)
            : this(data, destination, step)
        {
            _blocked = (destination == HDestination.Client)
               ? filters.InProcessFilters(Replacement, ref _repeat)
               : filters.OutProcessFilters(Replacement, ref _repeat);
        }
    }
}