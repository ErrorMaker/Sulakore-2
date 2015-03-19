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
        private readonly Stack<HMessage> _outPrevious, _inPrevious;
        private readonly IDictionary<int, List<Func<HMessage, HMessage, bool>>> _inProcessors, _outProcessors;
        private readonly IDictionary<ushort, Action<HMessage>> _outLocked, _inLocked, _inCallbacks, _outCallbacks;

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
                    _outLocked.Clear();
                    _inLocked.Clear();
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
                    _outPrevious.Clear();
                    _inPrevious.Clear();
                }
            }
        }
        public bool UpdateHeaders { get; set; }

        public HTriggers()
        {
            _inPrevious = new Stack<HMessage>();
            _outPrevious = new Stack<HMessage>();

            _inLocked = new Dictionary<ushort, Action<HMessage>>();
            _outLocked = new Dictionary<ushort, Action<HMessage>>();

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

        public void ProcessOutgoing(byte[] data)
        {
            var current = new HMessage(data, HDestination.Server);
            ProcessOutgoing(current);
        }
        public void ProcessOutgoing(HMessage current)
        {
            if (current == null || current.IsCorrupted) return;
            bool ignoreCurrent = false;

            try
            {
                if (_outCallbacks.ContainsKey(current.Header))
                    _outCallbacks[current.Header](current);

                if (_outPrevious.Count > 0 && CaptureEvents)
                {
                    HMessage previous = null;

                    if (_outPrevious.Count > 0)
                        previous = _outPrevious.Pop();

                    if (LockEvents && _outLocked.ContainsKey(current.Header))
                        _outLocked[current.Header](current); // These don't make sense, or do they?; Not sure why I put this particular check here.
                    else if (previous != null && (LockEvents && !_outLocked.ContainsKey(previous.Header)))
                        ignoreCurrent = ProcessOutgoing(current, previous);
                }
            }
            finally
            {
                if (!ignoreCurrent)
                {
                    current.Position = 0;

                    if (CaptureEvents)
                        _outPrevious.Push(current);
                }
            }

        }
        protected bool ProcessOutgoing(HMessage current, HMessage previous)
        {
            if (current.Length == 6)
            {
                // Range: 6
                if (TryProcessAvatarMenuClick(current, previous)) return true;
                if (TryProcessHostRoomExit(current, previous)) return true;
            }
            else if (current.Length >= 36 && current.Length <= 50)
            {
                //Range: 36 - 50
                if (TryProcessHostRaiseSign(current, previous)) return true;
                if (TryProcessHostRoomNavigate(current, previous)) return true;
            }
            return false;
        }

        public void ProcessIncoming(byte[] data)
        {
            var current = new HMessage(data, HDestination.Client);
            ProcessIncoming(current);
        }
        public void ProcessIncoming(HMessage current)
        {
            if (current == null || current.IsCorrupted) return;
            bool ignoreCurrent = false;

            try
            {
                if (_inCallbacks.ContainsKey(current.Header))
                    _inCallbacks[current.Header](current);

                if (_inPrevious.Count > 0 && CaptureEvents)
                {
                    HMessage previous = _inPrevious.Pop();

                    if (LockEvents && _inLocked.ContainsKey(current.Header))
                        _inLocked[current.Header](current);
                    else if (previous != null && (LockEvents && _inLocked.ContainsKey(previous.Header)))
                        ignoreCurrent = ProcessIncoming(current, previous);
                }
            }
            finally
            {
                if (!ignoreCurrent)
                {
                    current.Position = 0;

                    if (CaptureEvents)
                        _inPrevious.Push(current);
                }
            }
        }
        protected bool ProcessIncoming(HMessage current, HMessage previous)
        {
            if (current.Length == 6)
            {
                // Range: 6
                if (TryProcessPlayerKickHost(current, previous)) return true;
            }
            return false;
        }

        private bool TryProcessHostRoomExit(HMessage current, HMessage previous)
        {
            if (previous.Length != 2 || current.ReadInt(0) != -1) return false;

            if (UpdateHeaders)
                Outgoing.RoomExit = previous.Header;

            _outLocked[previous.Header] = RaiseOnHostRoomExit;
            RaiseOnHostRoomExit(previous);
            return true;
        }
        private bool TryProcessHostRaiseSign(HMessage current, HMessage previous)
        {
            bool isHostRaiseSign = false;
            if (current.CanReadAt<string>(22) && current.ReadString(22) == "sign")
            {
                if (UpdateHeaders)
                    Outgoing.RaiseSign = previous.Header;

                _outLocked[previous.Header] = RaiseOnHostRaiseSign;
                RaiseOnHostRaiseSign(previous);
                isHostRaiseSign = true;
            }
            return isHostRaiseSign;
        }
        private bool TryProcessPlayerKickHost(HMessage current, HMessage previous)
        {
            bool isPlayerKickHost = (current.ReadInt(0) == 4008);
            if (isPlayerKickHost)
            {
                if (UpdateHeaders)
                    Incoming.PlayerKickHost = current.Header;

                _inLocked[current.Header] = RaiseOnPlayerKickHost;
                RaiseOnPlayerKickHost(current);
            }
            return isPlayerKickHost;
        }
        private bool TryProcessAvatarMenuClick(HMessage current, HMessage previous)
        {
            if (!previous.CanReadAt<string>(22)) return false;
            switch (previous.ReadString(22))
            {
                case "sit":
                case "stand":
                {
                    if (UpdateHeaders)
                        Outgoing.ChangeStance = current.Header;

                    _outLocked[current.Header] = RaiseOnHostChangeStance;
                    RaiseOnHostChangeStance(current);
                    return true;
                }

                case "dance_stop":
                case "dance_start":
                {
                    if (UpdateHeaders)
                        Outgoing.Dance = current.Header;

                    _outLocked[current.Header] = RaiseOnHostDance;
                    RaiseOnHostDance(current);
                    return true;
                }

                case "wave":
                case "idle":
                case "laugh":
                case "blow_kiss":
                {
                    if (UpdateHeaders)
                        Outgoing.Gesture = current.Header;

                    _outLocked[current.Header] = RaiseOnHostGesture;
                    RaiseOnHostGesture(current);
                    return true;
                }
            }
            return false;
        }
        private bool TryProcessHostRoomNavigate(HMessage current, HMessage previous)
        {
            if (previous.Length >= 12 && current.CanReadAt<string>(0)
                && current.ReadString() == "Navigation")
            {
                current.ReadString(); // TODO: Check if all navigation request logs use a "go.offical" value.
                if (current.ReadString() != "go.official") return false;

                if (previous.ReadInt(0).ToString() == current.ReadString())
                {
                    if (UpdateHeaders)
                        Outgoing.RoomNavigate = previous.Header;

                    _outLocked[previous.Header] = RaiseOnHostRoomNavigate;
                    RaiseOnHostRoomNavigate(previous);
                    return true;
                }
            }
            return false;
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