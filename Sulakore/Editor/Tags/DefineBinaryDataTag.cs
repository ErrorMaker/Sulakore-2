using System;
using System.Collections.Generic;

namespace Sulakore.Editor.Tags
{
    public class DefineBinaryDataTag : IFlashTag
    {
        private readonly ShockwaveFlash _flash;

        public int Position { get; private set; }
        public RecordHeader Header { get; private set; }

        private byte[] _data;
        public byte[] Data
        {
            get { return _data; }
            set
            {
                if (value.Length == _data.Length)
                    _flash.ReplaceUI8Block(Position + 6, value);

                _data = value;
            }
        }

        public uint Reserved { get; set; }
        public ushort CharacterId { get; set; }

        public DefineBinaryDataTag(ShockwaveFlash flash, RecordHeader header)
        {
            _flash = flash;

            Position = flash.Position;
            Header = header;

            CharacterId = _flash.ReadUI16();
            Reserved = _flash.ReadUI32();
            _data = _flash.ReadUI8Block((int)header.Length - 6);
        }

        public byte[] ToBytes()
        {
            var buffer = new List<byte>(Data.Length + 6);
            buffer.AddRange(BitConverter.GetBytes(CharacterId));
            buffer.AddRange(BitConverter.GetBytes(Reserved));
            buffer.AddRange(Data);

            return ShockwaveFlash.ConstructTagData(Header.Tag, buffer.ToArray());
        }
    }
}