using System;
using System.Linq;
using System.Collections.Generic;

using Sulakore.Protocol;
using Sulakore.Habbo.Headers;

namespace Sulakore.Communication
{
    public class HTriggers : IDisposable
    {
        private bool _lockEvents, _captureEvents;

        private readonly Stack<HMessage> _previousOutgoing, _previousIncoming;
        private readonly IDictionary<ushort, Action<HMessage>> _lockedOut, _lockedIn, _inCallbacks, _outCallbacks;

        private static readonly string[] _gestureTypes;

        #region Incoming Game Event Handlers
        public event EventHandler<FurnitureDataLoadedEventArgs> FurnitureDataLoaded;
        protected virtual void OnFurnitureDataLoaded(FurnitureDataLoadedEventArgs e)
        {
            EventHandler<FurnitureDataLoadedEventArgs> handler = FurnitureDataLoaded;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnFurnitureDataLoaded(HMessage packet)
        {
            OnFurnitureDataLoaded(new FurnitureDataLoadedEventArgs(packet));
        }

        public event EventHandler<PlayerActionsDetectedEventArgs> PlayerActionsDetected;
        protected virtual void OnPlayerActionsDetected(PlayerActionsDetectedEventArgs e)
        {
            EventHandler<PlayerActionsDetectedEventArgs> handler = PlayerActionsDetected;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerActionsDetected(HMessage packet)
        {
            OnPlayerActionsDetected(new PlayerActionsDetectedEventArgs(packet));
        }

        public event EventHandler<PlayerChangeDataEventArgs> PlayerChangeData;
        protected virtual void OnPlayerChangeData(PlayerChangeDataEventArgs e)
        {
            EventHandler<PlayerChangeDataEventArgs> handler = PlayerChangeData;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerChangeData(HMessage packet)
        {
            OnPlayerChangeData(new PlayerChangeDataEventArgs(packet));
        }

        public event EventHandler<PlayerDanceEventArgs> PlayerDance;
        protected virtual void OnPlayerDance(PlayerDanceEventArgs e)
        {
            EventHandler<PlayerDanceEventArgs> handler = PlayerDance;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerDance(HMessage packet)
        {
            OnPlayerDance(new PlayerDanceEventArgs(packet));
        }

        public event EventHandler<PlayerDataLoadedEventArgs> PlayerDataLoaded;
        protected virtual void OnPlayerDataLoaded(PlayerDataLoadedEventArgs e)
        {
            EventHandler<PlayerDataLoadedEventArgs> handler = PlayerDataLoaded;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerDataLoaded(HMessage packet)
        {
            OnPlayerDataLoaded(new PlayerDataLoadedEventArgs(packet));
        }

        public event EventHandler<PlayerDropFurnitureEventArgs> PlayerDropFurniture;
        protected virtual void OnPlayerDropFurniture(PlayerDropFurnitureEventArgs e)
        {
            EventHandler<PlayerDropFurnitureEventArgs> handler = PlayerDropFurniture;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerDropFurniture(HMessage packet)
        {
            OnPlayerDropFurniture(new PlayerDropFurnitureEventArgs(packet));
        }

        public event EventHandler<PlayerGestureEventArgs> PlayerGesture;
        protected virtual void OnPlayerGesture(PlayerGestureEventArgs e)
        {
            EventHandler<PlayerGestureEventArgs> handler = PlayerGesture;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerGesture(HMessage packet)
        {
            OnPlayerGesture(new PlayerGestureEventArgs(packet));
        }

        public event EventHandler<PlayerKickHostEventArgs> PlayerKickHost;
        protected virtual void OnPlayerKickHost(PlayerKickHostEventArgs e)
        {
            EventHandler<PlayerKickHostEventArgs> handler = PlayerKickHost;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerKickHost(HMessage packet)
        {
            OnPlayerKickHost(new PlayerKickHostEventArgs(packet));
        }

        public event EventHandler<PlayerMoveFurnitureEventArgs> PlayerMoveFurniture;
        protected virtual void OnPlayerMoveFurniture(PlayerMoveFurnitureEventArgs e)
        {
            EventHandler<PlayerMoveFurnitureEventArgs> handler = PlayerMoveFurniture;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnPlayerMoveFurniture(HMessage packet)
        {
            OnPlayerMoveFurniture(new PlayerMoveFurnitureEventArgs(packet));
        }
        #endregion
        #region Outgoing Game Event Handlers
        public event EventHandler<HostBanPlayerEventArgs> HostBanPlayer;
        protected virtual void OnHostBanPlayer(HostBanPlayerEventArgs e)
        {
            EventHandler<HostBanPlayerEventArgs> handler = HostBanPlayer;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostBanPlayer(HMessage packet)
        {
            OnHostBanPlayer(new HostBanPlayerEventArgs(packet));
        }

        public event EventHandler<HostChangeClothesEventArgs> HostChangeClothes;
        protected virtual void OnHostChangeClothes(HostChangeClothesEventArgs e)
        {
            EventHandler<HostChangeClothesEventArgs> handler = HostChangeClothes;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostChangeClothes(HMessage packet)
        {
            OnHostChangeClothes(new HostChangeClothesEventArgs(packet));
        }

        public event EventHandler<HostChangeMottoEventArgs> HostChangeMotto;
        protected virtual void OnHostChangeMotto(HostChangeMottoEventArgs e)
        {
            EventHandler<HostChangeMottoEventArgs> handler = HostChangeMotto;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostChangeMotto(HMessage packet)
        {
            OnHostChangeMotto(new HostChangeMottoEventArgs(packet));
        }

        public event EventHandler<HostChangeStanceEventArgs> HostChangeStance;
        protected virtual void OnHostChangeStance(HostChangeStanceEventArgs e)
        {
            EventHandler<HostChangeStanceEventArgs> handler = HostChangeStance;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostChangeStance(HMessage packet)
        {
            OnHostChangeStance(new HostChangeStanceEventArgs(packet));
        }

        public event EventHandler<HostClickPlayerEventArgs> HostClickPlayer;
        protected virtual void OnHostClickPlayer(HostClickPlayerEventArgs e)
        {
            EventHandler<HostClickPlayerEventArgs> handler = HostClickPlayer;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostClickPlayer(HMessage packet)
        {
            OnHostClickPlayer(new HostClickPlayerEventArgs(packet));
        }

        public event EventHandler<HostDanceEventArgs> HostDance;
        protected virtual void OnHostDance(HostDanceEventArgs e)
        {
            EventHandler<HostDanceEventArgs> handler = HostDance;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostDance(HMessage packet)
        {
            OnHostDance(new HostDanceEventArgs(packet));
        }

        public event EventHandler<HostGestureEventArgs> HostGesture;
        protected virtual void OnHostGesture(HostGestureEventArgs e)
        {
            EventHandler<HostGestureEventArgs> handler = HostGesture;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostGesture(HMessage packet)
        {
            OnHostGesture(new HostGestureEventArgs(packet));
        }

        public event EventHandler<HostKickPlayerEventArgs> HostKickPlayer;
        protected virtual void OnHostKickPlayer(HostKickPlayerEventArgs e)
        {
            EventHandler<HostKickPlayerEventArgs> handler = HostKickPlayer;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostKickPlayer(HMessage packet)
        {
            OnHostKickPlayer(new HostKickPlayerEventArgs(packet));
        }

        public event EventHandler<HostMoveFurnitureEventArgs> HostMoveFurniture;
        protected virtual void OnHostMoveFurniture(HostMoveFurnitureEventArgs e)
        {
            EventHandler<HostMoveFurnitureEventArgs> handler = HostMoveFurniture;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostMoveFurniture(HMessage packet)
        {
            OnHostMoveFurniture(new HostMoveFurnitureEventArgs(packet));
        }

        public event EventHandler<HostMutePlayerEventArgs> HostMutePlayer;
        protected virtual void OnHostMutePlayer(HostMutePlayerEventArgs e)
        {
            EventHandler<HostMutePlayerEventArgs> handler = HostMutePlayer;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostMutePlayer(HMessage packet)
        {
            OnHostMutePlayer(new HostMutePlayerEventArgs(packet));
        }

        public event EventHandler<HostRaiseSignEventArgs> HostRaiseSign;
        protected virtual void OnHostRaiseSign(HostRaiseSignEventArgs e)
        {
            EventHandler<HostRaiseSignEventArgs> handler = HostRaiseSign;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostRaiseSign(HMessage packet)
        {
            OnHostRaiseSign(new HostRaiseSignEventArgs(packet));
        }

        public event EventHandler<HostRoomExitEventArgs> HostRoomExit;
        protected virtual void OnHostRoomExit(HostRoomExitEventArgs e)
        {
            EventHandler<HostRoomExitEventArgs> handler = HostRoomExit;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostRoomExit(HMessage packet)
        {
            OnHostRoomExit(new HostRoomExitEventArgs(packet));
        }

        public event EventHandler<HostRoomNavigateEventArgs> HostRoomNavigate;
        protected virtual void OnHostRoomNavigate(HostRoomNavigateEventArgs e)
        {
            EventHandler<HostRoomNavigateEventArgs> handler = HostRoomNavigate;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostRoomNavigate(HMessage packet)
        {
            OnHostRoomNavigate(new HostRoomNavigateEventArgs(packet));
        }

        public event EventHandler<HostSayEventArgs> HostSay;
        protected virtual void OnHostSay(HostSayEventArgs e)
        {
            EventHandler<HostSayEventArgs> handler = HostSay;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostSay(HMessage packet)
        {
            OnHostSay(new HostSayEventArgs(packet));
        }

        public event EventHandler<HostShoutEventArgs> HostShout;
        protected virtual void OnHostShout(HostShoutEventArgs e)
        {
            EventHandler<HostShoutEventArgs> handler = HostShout;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostShout(HMessage packet)
        {
            OnHostShout(new HostShoutEventArgs(packet));
        }

        public event EventHandler<HostTradePlayerEventArgs> HostTradePlayer;
        protected virtual void OnHostTradePlayer(HostTradePlayerEventArgs e)
        {
            EventHandler<HostTradePlayerEventArgs> handler = HostTradePlayer;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostTradePlayer(HMessage packet)
        {
            OnHostTradePlayer(new HostTradePlayerEventArgs(packet));
        }

        public event EventHandler<HostWalkEventArgs> HostWalk;
        protected virtual void OnHostWalk(HostWalkEventArgs e)
        {
            EventHandler<HostWalkEventArgs> handler = HostWalk;
            if (handler != null) handler(this, e);
        }
        protected void RaiseOnHostWalk(HMessage packet)
        {
            OnHostWalk(new HostWalkEventArgs(packet));
        }
        #endregion

        public bool LockEvents
        {
            get { return _lockEvents; }
            set
            {
                if (value == _lockEvents) return;

                if (!(_lockEvents = value))
                {
                    _lockedOut.Clear();
                    _lockedIn.Clear();
                }
            }
        }
        public bool CaptureEvents
        {
            get { return _captureEvents; }
            set
            {
                if (value == _captureEvents) return;

                if (!(_captureEvents = value))
                {
                    _previousOutgoing.Clear();
                    _previousIncoming.Clear();
                }
            }
        }
        public bool UpdateHeaders { get; set; }

        static HTriggers()
        {
            _gestureTypes = new[] { "wave", "idle", "blow_kiss", "laugh" };
        }
        public HTriggers()
        {
            _previousOutgoing = new Stack<HMessage>();
            _previousIncoming = new Stack<HMessage>();

            _lockedIn = new Dictionary<ushort, Action<HMessage>>();
            _lockedOut = new Dictionary<ushort, Action<HMessage>>();

            _inCallbacks = new Dictionary<ushort, Action<HMessage>>();
            _outCallbacks = new Dictionary<ushort, Action<HMessage>>();
        }

        public void InDetach()
        {
            _inCallbacks.Clear();
        }
        public void InDetach(ushort header)
        {
            if (_inCallbacks.ContainsKey(header))
                _inCallbacks.Remove(header);
        }
        public void InAttach(ushort header, Action<HMessage> callback)
        {
            _inCallbacks[header] = callback;
        }

        public void OutDetach()
        {
            _outCallbacks.Clear();
        }
        public void OutDetach(ushort header)
        {
            if (_outCallbacks.ContainsKey(header))
                _outCallbacks.Remove(header);
        }
        public void OutAttach(ushort header, Action<HMessage> callback)
        {
            _outCallbacks[header] = callback;
        }

        public virtual void ProcessOutgoing(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Server);
            if (_outCallbacks.Count > 0 && _outCallbacks.ContainsKey(packet.Header))
            {
                _outCallbacks[packet.Header](packet);
                packet.Position = 0;
            }
            if (!CaptureEvents) return;
            try
            {
                HMessage previousOutgoing = (_previousOutgoing.Count > 0 ? _previousOutgoing.Pop() : null);
                if (LockEvents && _lockedOut.ContainsKey(packet.Header))
                {
                    _lockedOut[packet.Header](packet);
                    packet = null;
                }
                else ProcessOutgoing(previousOutgoing, ref packet);
            }
            catch { packet = null; }
            finally
            {
                if (packet != null)
                {
                    packet.Position = 0;
                    _previousOutgoing.Push(packet);
                }
            }
        }
        private void ProcessOutgoing(HMessage previous, ref HMessage current)
        {
            int length = current.Length;
            switch (length)
            {
                case 2:
                {
                    break;
                }
                case 4:
                {
                    break;
                }
                case 6:
                {
                    if (previous != null)
                    {
                        if (previous.Length >= 28)
                        {
                            int position = 0;
                            previous.ReadString(ref position);
                            previous.ReadString(ref position);
                            if (_gestureTypes.Contains(previous.ReadString(position)))
                            {
                                if (UpdateHeaders)
                                    Outgoing.Gesture = current.Header;

                                _lockedOut[current.Header] = RaiseOnHostGesture;
                                OnHostGesture(new HostGestureEventArgs(current));
                                current = null;
                            }
                        }
                    }

                    int int_value = current.ReadInt(0);
                    switch (int_value)
                    {
                        case -1:
                        {
                            if (previous == null) break;

                            if (UpdateHeaders)
                                Outgoing.RoomExit = previous.Header;

                            _lockedOut[previous.Header] = RaiseOnHostRoomExit;
                            OnHostRoomExit(new HostRoomExitEventArgs(previous));
                            current = null;
                            break;
                        }
                    }
                    break;
                }
                case 10:
                {
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        public virtual void ProcessIncoming(byte[] data)
        {
            var packet = new HMessage(data, HDestination.Client);
            if (_inCallbacks.Count > 0 && _inCallbacks.ContainsKey(packet.Header))
            {
                _inCallbacks[packet.Header](packet);
                packet.Position = 0;
            }
            if (!CaptureEvents) return;
            try
            {
                HMessage previousIncoming = (_previousIncoming.Count > 0 ? _previousIncoming.Pop() : null);
                if (LockEvents && _lockedIn.ContainsKey(packet.Header))
                {
                    _lockedIn[packet.Header](packet);
                    packet = null;
                }
                else ProcessIncoming(previousIncoming, ref packet);
            }
            catch { packet = null; }
            finally
            {
                if (packet != null)
                {
                    packet.Position = 0;
                    _previousIncoming.Push(packet);
                }
            }
        }
        private void ProcessIncoming(HMessage previous, ref HMessage current)
        {
            int length = current.Length;
            switch (length)
            {
                case 2:
                {
                    break;
                }
                case 4:
                {
                    break;
                }
                case 6:
                {
                    int intValue = current.ReadInt(0);
                    switch (intValue)
                    {
                        case 4008:
                        {
                            if (UpdateHeaders)
                                Incoming.PlayerKickHost = current.Header;

                            _lockedIn[current.Header] = RaiseOnPlayerKickHost;
                            OnPlayerKickHost(new PlayerKickHostEventArgs(current));
                            current = null;
                            break;
                        }
                    }
                    break;
                }
                case 10:
                {
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CaptureEvents = LockEvents = false;
                _inCallbacks.Clear();
                _outCallbacks.Clear();
            }

            SKore.Unsubscribe(ref HostBanPlayer);
            SKore.Unsubscribe(ref HostChangeClothes);
            SKore.Unsubscribe(ref HostChangeMotto);
            SKore.Unsubscribe(ref HostChangeStance);
            SKore.Unsubscribe(ref HostClickPlayer);
            SKore.Unsubscribe(ref HostDance);
            SKore.Unsubscribe(ref HostGesture);
            SKore.Unsubscribe(ref HostKickPlayer);
            SKore.Unsubscribe(ref HostMoveFurniture);
            SKore.Unsubscribe(ref HostMutePlayer);
            SKore.Unsubscribe(ref HostRaiseSign);
            SKore.Unsubscribe(ref HostRoomExit);
            SKore.Unsubscribe(ref HostRoomNavigate);
            SKore.Unsubscribe(ref HostSay);
            SKore.Unsubscribe(ref HostShout);
            SKore.Unsubscribe(ref HostTradePlayer);
            SKore.Unsubscribe(ref HostWalk);

            SKore.Unsubscribe(ref FurnitureDataLoaded);
            SKore.Unsubscribe(ref PlayerActionsDetected);
            SKore.Unsubscribe(ref PlayerChangeData);
            SKore.Unsubscribe(ref PlayerDance);
            SKore.Unsubscribe(ref PlayerDataLoaded);
            SKore.Unsubscribe(ref PlayerDropFurniture);
            SKore.Unsubscribe(ref PlayerGesture);
            SKore.Unsubscribe(ref PlayerKickHost);
            SKore.Unsubscribe(ref PlayerMoveFurniture);
        }
    }
}