using System;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    public class HFilters
    {
        private readonly IHConnection _connection;
        private readonly IList<ushort> _inBlockedHeaders, _outBlockedHeaders;
        private readonly IDictionary<ushort, Func<HMessage, bool>> _inBlockConditions, _outBlockConditions;

        public HFilters(IHConnection connection)
        {
            _connection = connection;

            _inBlockedHeaders = new List<ushort>();
            _outBlockedHeaders = new List<ushort>();

            _inBlockConditions = new Dictionary<ushort, Func<HMessage, bool>>();
            _outBlockConditions = new Dictionary<ushort, Func<HMessage, bool>>();
        }

        public void InUnblock()
        {
            Unblock(HDestination.Client);
        }
        public void InUnblock(ushort header)
        {
            Unblock(header, HDestination.Client);
        }
        public void InBlock(ushort header)
        {
            Block(header, HDestination.Client);
        }
        public void InBlock(ushort header, Func<HMessage, bool> condition)
        {
            Block(header, HDestination.Client, condition);
        }

        public void OutUnblock()
        {
            Unblock(HDestination.Server);
        }
        public void OutUnblock(ushort header)
        {
            Unblock(header, HDestination.Server);
        }
        public void OutBlock(ushort header)
        {
            Block(header, HDestination.Server);
        }
        public void OutBlock(ushort header, Func<HMessage, bool> condition)
        {
            Block(header, HDestination.Server, condition);
        }

        public virtual void Unblock(HDestination destination)
        {
            bool isIncoming = (destination == HDestination.Client);
            (isIncoming ? _inBlockedHeaders : _outBlockedHeaders).Clear();
            (isIncoming ? _inBlockConditions : _outBlockConditions).Clear();
        }
        public virtual void Unblock(ushort header, HDestination destination)
        {
            bool isIncoming = (destination == HDestination.Client);
            IList<ushort> blockedHeaders = (isIncoming ? _inBlockedHeaders : _outBlockedHeaders);
            IDictionary<ushort, Func<HMessage, bool>> blockConditions = (isIncoming ? _inBlockConditions : _outBlockConditions);

            if (blockedHeaders.Contains(header)) blockedHeaders.Remove(header);
            else if (blockConditions.ContainsKey(header)) blockConditions.Remove(header);
        }
        public virtual void Block(ushort header, HDestination destination)
        {
            Unblock(header, destination);

            (destination == HDestination.Client
                ? _inBlockedHeaders : _outBlockedHeaders).Add(header);
        }
        public virtual void Block(ushort header, HDestination destination, Func<HMessage, bool> condition)
        {
            Unblock(header, destination);

            (destination == HDestination.Client
                ? _inBlockConditions : _outBlockConditions).Add(header, condition);
        }

        /// <summary>
        /// Determines whether the incoming packet should be blocked, otherwise attempts to apply the filters related to the packet.
        /// </summary>
        /// <param name="packet">The incoming packet to process.</param>
        /// <param name="repeat">The value type to update with the number of times the packet should be repeated.</param>
        /// <returns>true if the packet should be blocked; otherwise false.</returns>
        public virtual bool InProcessFilters(HMessage packet, ref int repeat)
        {
            if (_inBlockedHeaders.Contains(packet.Header) || (_inBlockConditions.ContainsKey(packet.Header)
                && _inBlockConditions[packet.Header](packet))) return true;

            return false;
        }
        /// <summary>
        /// Determines whether the outgoing packet should be blocked, otherwise attempts to apply the filters related to the packet.
        /// </summary>
        /// <param name="packet">The outgoing packet to process.</param>
        /// <param name="repeat">The value type to update with the number of times the packet should be repeated.</param>
        /// <returns>true if the packet should be blocked; otherwise false.</returns>
        public virtual bool OutProcessFilters(HMessage packet, ref int repeat)
        {
            if (_outBlockedHeaders.Contains(packet.Header) || (_outBlockConditions.ContainsKey(packet.Header)
                && _outBlockConditions[packet.Header](packet))) return true;

            return false;
        }
    }
}