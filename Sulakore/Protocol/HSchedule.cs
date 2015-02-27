using System;
using System.Timers;

namespace Sulakore.Protocol
{
    public class HSchedule : IDisposable
    {
        public event EventHandler<HScheduleTriggeredEventArgs> ScheduleTriggered;
        protected virtual void OnScheduleTriggered(HScheduleTriggeredEventArgs e)
        {
            EventHandler<HScheduleTriggeredEventArgs> handler = ScheduleTriggered;
            if (handler != null) handler(this, e);
            if (e.Cancel) IsRunning = false;
        }

        private readonly object _tickerLock;
        private readonly System.Timers.Timer _ticker;

        public int Interval
        {
            get { return (int)_ticker.Interval; }
            set { _ticker.Interval = value; }
        }
        public int Burst { get; set; }
        public HMessage Packet { get; set; }
        public bool IsRunning { get; private set; }

        public HSchedule(HMessage packet, int interval, int burst)
        {
            _ticker = new System.Timers.Timer(interval);
            _ticker.Elapsed += Ticker_Elapsed;

            _tickerLock = new object();

            Packet = packet;
            Burst = burst;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _ticker.Stop();
            IsRunning = false;
        }
        public void Start()
        {
            if (IsRunning) return;

            _ticker.Start();
            IsRunning = true;
        }
        public void Toggle()
        {
            if (IsRunning) Stop();
            else Start();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            SKore.Unsubscribe(ref ScheduleTriggered);

            if (disposing)
            {
                Stop();
                _ticker.Dispose();
            }
        }

        private void Ticker_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_tickerLock)
            {
                _ticker.Stop();
                int tmpBurst = Burst, burstCount;
                for (int i = 0; i < tmpBurst && IsRunning; i++)
                {
                    burstCount = i + 1;

                    OnScheduleTriggered(new HScheduleTriggeredEventArgs(Packet,
                        burstCount, tmpBurst - burstCount, burstCount >= tmpBurst));
                }
                if (IsRunning) _ticker.Start();
            }
        }
    }
}