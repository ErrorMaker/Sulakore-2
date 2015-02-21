using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sulakore.Habbo.Headers
{
    [Serializable]
    public struct Outgoing : ISerializable
    {
        private static Outgoing _instance;
        private static readonly PropertyInfo[] _props;
        private static readonly BinaryFormatter _binFormatter;

        public const ushort ClientHello = 4000;

        public static ushort InitiateHandshake { get; set; }
        public static ushort ClientPublicKey { get; set; }
        public static ushort FlashClientUrl { get; set; }
        public static ushort ClientSsoTicket { get; set; }

        public static ushort ShopObjectGet { get; set; }
        public static ushort PetScratch { get; set; }
        public static ushort PlayerEffect { get; set; }
        public static ushort BanPlayer { get; set; }
        public static ushort ChangeClothes { get; set; }
        public static ushort ChangeMotto { get; set; }
        public static ushort ChangeStance { get; set; }
        public static ushort ClickPlayer { get; set; }
        public static ushort Dance { get; set; }
        public static ushort Gesture { get; set; }
        public static ushort KickPlayer { get; set; }
        public static ushort MoveFurniture { get; set; }
        public static ushort MutePlayer { get; set; }
        public static ushort RaiseSign { get; set; }
        public static ushort RoomExit { get; set; }
        public static ushort RoomNavigate { get; set; }
        public static ushort Say { get; set; }
        public static ushort Shout { get; set; }
        public static ushort Whisper { get; set; }
        public static ushort TradePlayer { get; set; }
        public static ushort Walk { get; set; }

        static Outgoing()
        {
            _instance = new Outgoing();
            _binFormatter = new BinaryFormatter();
            _props = typeof(Outgoing).GetProperties();
        }
        public Outgoing(SerializationInfo info, StreamingContext context)
        {
            ushort currentHeader = 0;
            foreach (PropertyInfo prop in _props)
            {
                currentHeader = info.GetUInt16(prop.Name);
                if (currentHeader == 0) continue;

                prop.SetValue(_instance, currentHeader, null);
            }
        }

        public static void Save(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Create))
                _binFormatter.Serialize(fileStream, _instance);
        }
        public static void Load(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
                _instance = (Outgoing)_binFormatter.Deserialize(fileStream);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (PropertyInfo prop in _props)
                info.AddValue(prop.Name, prop.GetValue(_instance, null), typeof(UInt16));
        }
    }
}