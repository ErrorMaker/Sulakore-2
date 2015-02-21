using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Sulakore.Habbo.Headers
{
    [Serializable]
    public struct Incoming : ISerializable
    {
        private static Incoming _instance;
        private static readonly PropertyInfo[] _props;
        private static readonly BinaryFormatter _binFormatter;

        public const ushort ClientDisconnect = 4000;

        public static ushort LocalAlert { get; set; }
        public static ushort FloorLoaded { get; set; }
        public static ushort FurnitureDataLoaded { get; set; }
        public static ushort PlayerChangeData { get; set; }
        public static ushort PlayerChangeStance { get; set; }
        public static ushort PlayerDance { get; set; }
        public static ushort PlayerDataLoaded { get; set; }
        public static ushort PlayerDropFurniture { get; set; }
        public static ushort PlayerGesture { get; set; }
        public static ushort PlayerKickHost { get; set; }
        public static ushort PlayerMoveFurniture { get; set; }
        public static ushort PlayerSay { get; set; }
        public static ushort PlayerShout { get; set; }
        public static ushort PlayerWhisper { get; set; }

        static Incoming()
        {
            _instance = new Incoming();
            _binFormatter = new BinaryFormatter();
            _props = typeof(Incoming).GetProperties();
        }
        public Incoming(SerializationInfo info, StreamingContext context)
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
                _instance = (Incoming)_binFormatter.Deserialize(fileStream);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (PropertyInfo prop in _props)
                info.AddValue(prop.Name, prop.GetValue(_instance, null), typeof(UInt16));
        }
    }
}