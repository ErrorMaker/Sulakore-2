using System.ComponentModel;

namespace Sulakore.Protocol
{
    public class HScheduleTriggeredEventArgs : CancelEventArgs
    {
        public int BurstLeft { get; private set; }
        public int BurstCount { get; private set; }
        public HMessage Packet { get; private set; }
        public bool IsFinalBurst { get; private set; }

        public HScheduleTriggeredEventArgs(HMessage packet, int burstCount, int burstLeft, bool isFinalBurst)
        {
            Packet = packet;
            BurstCount = burstCount;
            BurstLeft = burstLeft;
            IsFinalBurst = isFinalBurst;
        }

        public override string ToString()
        {
            return string.Format("Packet: {0}, BurstCount: {1}, BurstLeft: {2}, IsFinalBurst: {3}",
                Packet, BurstCount, BurstLeft, IsFinalBurst);
        }
    }
}