using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Sulakore.Habbo.Headers
{
    public class Incoming
    {
        private static readonly Type _incomingType;
        private static readonly DataContractJsonSerializer _serializer;

        private static readonly Incoming _global;
        public static Incoming Global
        {
            get { return _global; }
        }

        public const ushort CLIENT_DISCONNECT = 4000;

        public static ushort RoomMapLoaded { get; set; }
        public static ushort LocalHotelAlert { get; set; }
        public static ushort GlobalHotelAlert { get; set; }

        public static ushort PlayerDataLoaded { get; set; }
        public static ushort FurnitureDataLoaded { get; set; }

        public static ushort PlayerChangeData { get; set; }
        public static ushort PlayerChangeStance { get; set; }

        public static ushort PlayerDance { get; set; }
        public static ushort PlayerGesture { get; set; }
        public static ushort PlayerKickHost { get; set; }

        public static ushort PlayerDropFurniture { get; set; }
        public static ushort PlayerMoveFurniture { get; set; }

        public static ushort PlayerSay { get; set; }
        public static ushort PlayerShout { get; set; }
        public static ushort PlayerWhisper { get; set; }

        static Incoming()
        {
            _global = new Incoming();
            _incomingType = typeof(Incoming);
            _serializer = new DataContractJsonSerializer(_incomingType);
        }

        public void Save(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                _serializer.WriteObject(fileStream, this);
        }
        public static Incoming Load(string path)
        {
            using (var fileStream = File.Open(path, FileMode.Open))
                return (Incoming)_serializer.ReadObject(fileStream);
        }
    }
}