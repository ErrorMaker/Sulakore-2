using System;
using System.IO;
using System.Text;
using Sulakore.Editor.Tags;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sulakore.Editor
{
    public class ShockwaveFlash
    {
        private byte _currentByte;
        private int _position, _bitPosition;
        private byte[] _buffer;

        private readonly UTF8Encoding _encoding;
        private readonly byte[] _header, _trailer;

        private static readonly float[] _powers;
        private static readonly uint[] _bitValues;

        private const string FWS = "FWS", CWS = "CWS", ZWS = "ZWS";

        public int Position
        {
            get { return _position; }
        }

        public string Signature { get; private set; }
        public int Version { get; private set; }
        public uint FileLength { get; private set; }
        public Rect FrameSize { get; private set; }
        public int FrameRate { get; private set; }
        public int FrameCount { get; private set; }

        public bool IsCompressed { get; private set; }

        public IList<IFlashTag> Tags { get; private set; }

        static ShockwaveFlash()
        {
            _bitValues = new uint[32];
            for (byte power = 0; power < 32; power++)
                _bitValues[power] = (uint)(1 << power);

            _powers = new float[32];
            for (byte power = 0; power < 32; power++)
                _powers[power] = (float)Math.Pow(2, power - 16);
        }
        public ShockwaveFlash(byte[] data)
        {
            if (data.Length < 8)
                throw new Exception("Not enough data to parse file header.");

            _header = new byte[2];
            _trailer = new byte[4];

            _buffer = data;
            Tags = new List<IFlashTag>();
            _encoding = new UTF8Encoding();

            Signature = _encoding.GetString(ReadUI8Block(3));
            Version = ReadUI8();
            FileLength = ReadUI32();

            switch (Signature)
            {
                case CWS:
                {
                    Decompress();
                    IsCompressed = false;
                    break;
                }
                case FWS: IsCompressed = false; break;
                case ZWS: throw new Exception("Unable to decompress a file that was initially compressed via the LZMA standard.");
                default: throw new Exception("Invalid Macromedia Flash file signature.");
            }

            if (!IsCompressed)
            {
                FrameSize = new Rect(this);
                FrameRate = (ReadUI16() >> 8);
                FrameCount = ReadUI16();
                ExtractTags();
            }
        }
        public ShockwaveFlash(string path)
            : this(File.ReadAllBytes(path))
        { }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        private void Compress()
        {
            using (var compressedStream = new MemoryStream())
            {
                compressedStream.Write(_buffer, 0, 8);
                compressedStream.Write(_header, 0, _header.Length);

                using (var compresser = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                    compresser.Write(_buffer, 8, _buffer.Length - 8);

                compressedStream.Write(_trailer, 0, _trailer.Length);
                _buffer = compressedStream.ToArray();
                _buffer[0] = (byte)'C';
            }
        }
        private void Decompress()
        {
            Array.Copy(_buffer, 8, _header, 0, 2);
            Array.Copy(_buffer, _buffer.Length - 4, _trailer, 0, 4);

            using (var decompressedStream = new MemoryStream((int)FileLength))
            using (var bufferStream = new MemoryStream(_buffer, 10, _buffer.Length - 10))
            using (var decompresser = new DeflateStream(bufferStream, CompressionMode.Decompress))
            {
                decompressedStream.Write(_buffer, 0, 8);
                decompresser.CopyTo(decompressedStream);

                _buffer = decompressedStream.ToArray();
                _buffer[0] = (byte)'F';
            }
        }
        private void ExtractTags()
        {
            RecordHeader header;
            int expectedPosition;
            do
            {
                header = new RecordHeader(this);
                expectedPosition = (_position + (int)header.Length);

                switch (header.Tag)
                {
                    case FlashTagType.DoInitAction:
                    case FlashTagType.DefineBitsLossless2:
                    case FlashTagType.DefineBitsJPEG3: Tags.Add(new UnsupportedTag(this, header, true)); break;

                    case FlashTagType.DefineBinaryData: Tags.Add(new DefineBinaryDataTag(this, header)); break;

                    default: Tags.Add(new UnsupportedTag(this, header)); break;
                }

                if (_position != expectedPosition)
                    throw new Exception(string.Format("{0} was not correctly parsed.", header.Tag));
            }
            while (header.Tag != FlashTagType.End);
        }

        private static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        #region Reading Methods
        private bool ReadBit()
        {
            if (_bitPosition > 7)
            {
                _currentByte = ReadUI8();
                _bitPosition = 0;
            }
            return ((_currentByte & _bitValues[(7 - _bitPosition++)]) != 0);
        }
        public int ReadSB(int nBits)
        {
            if (nBits < 1) return 0;

            int result = 0;
            if (ReadBit())
                result -= (int)_bitValues[nBits - 1];

            for (int index = nBits - 2; index > -1; index--)
                if (ReadBit())
                    result |= (int)_bitValues[index];

            return result;
        }
        public uint ReadUB(int nBits)
        {
            if (nBits < 1) return 0;

            uint result = 0;
            for (int index = nBits - 1; index > -1; index--)
                if (ReadBit())
                    result |= _bitValues[index];

            return result;
        }
        public float ReadFB(int nBits)
        {
            if (nBits < 1) return 0;

            float result = 0;
            if (ReadBit())
                result -= _powers[nBits - 1];

            for (int index = nBits - 1; index > 0; index--)
                if (ReadBit())
                    result += _powers[index - 1];

            return result;
        }

        public sbyte ReadSI8()
        {
            return ReadSI8(ref _position);
        }
        public sbyte ReadSI8(int index)
        {
            return ReadSI8(ref index);
        }
        public sbyte ReadSI8(ref int index)
        {
            return (sbyte)ReadUI8(ref index);
        }
        public sbyte[] ReadSI8Block(int length)
        {
            return ReadSI8Block(length, ref _position);
        }
        public sbyte[] ReadSI8Block(int length, int index)
        {
            return ReadSI8Block(length, ref index);
        }
        public sbyte[] ReadSI8Block(int length, ref int index)
        {
            sbyte[] buffer = new sbyte[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadSI8(ref index);

            return buffer;
        }

        public byte ReadUI8()
        {
            return ReadUI8(ref _position);
        }
        public byte ReadUI8(int index)
        {
            return ReadUI8(ref index);
        }
        public byte ReadUI8(ref int index)
        {
            _bitPosition = 8;
            return _buffer[index++];
        }
        public byte[] ReadUI8Block(int length)
        {
            return ReadUI8Block(length, ref _position);
        }
        public byte[] ReadUI8Block(int length, int index)
        {
            return ReadUI8Block(length, ref index);
        }
        public byte[] ReadUI8Block(int length, ref int index)
        {
            byte[] buffer = new byte[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadUI8(ref index);

            return buffer;
        }

        public short ReadSI16()
        {
            return ReadSI16(ref _position);
        }
        public short ReadSI16(int index)
        {
            return ReadSI16(ref index);
        }
        public short ReadSI16(ref int index)
        {
            return (short)ReadUI16(ref index);
        }
        public short[] ReadSI16Block(int length)
        {
            return ReadSI16Block(length, ref _position);
        }
        public short[] ReadSI16Block(int length, int index)
        {
            return ReadSI16Block(length, ref index);
        }
        public short[] ReadSI16Block(int length, ref int index)
        {
            short[] buffer = new short[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadSI16(ref index);

            return buffer;
        }

        public ushort ReadUI16()
        {
            return ReadUI16(ref _position);
        }
        public ushort ReadUI16(int index)
        {
            return ReadUI16(ref index);
        }
        public ushort ReadUI16(ref int index)
        {
            ushort value = 0;
            value |= (ushort)ReadUI8(ref index);
            value |= (ushort)(ReadUI8(ref index) << 8);
            return value;
        }
        public ushort[] ReadUI16Block(int length)
        {
            return ReadUI16Block(length, ref _position);
        }
        public ushort[] ReadUI16Block(int length, int index)
        {
            return ReadUI16Block(length, ref index);
        }
        public ushort[] ReadUI16Block(int length, ref int index)
        {
            ushort[] buffer = new ushort[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadUI16(ref index);

            return buffer;
        }

        public int ReadSI32()
        {
            return ReadSI32(ref _position);
        }
        public int ReadSI32(int index)
        {
            return ReadSI32(ref index);
        }
        public int ReadSI32(ref int index)
        {
            return (int)ReadUI32(ref index);
        }
        public int[] ReadSI32Block(int length)
        {
            return ReadSI32Block(length, ref _position);
        }
        public int[] ReadSI32Block(int length, int index)
        {
            return ReadSI32Block(length, ref index);
        }
        public int[] ReadSI32Block(int length, ref int index)
        {
            int[] buffer = new int[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadSI32(ref index);

            return buffer;
        }

        public uint ReadUI32()
        {
            return ReadUI32(ref _position);
        }
        public uint ReadUI32(int index)
        {
            return ReadUI32(ref index);
        }
        public uint ReadUI32(ref int index)
        {
            uint value = 0;
            value |= (uint)ReadUI8(ref index);
            value |= (uint)(ReadUI8(ref index) << 8);
            value |= (uint)(ReadUI8(ref index) << 16);
            value |= (uint)(ReadUI8(ref index) << 24);
            return value;
        }
        public uint[] ReadUI32Block(int length)
        {
            return ReadUI32Block(length, ref _position);
        }
        public uint[] ReadUI32Block(int length, int index)
        {
            return ReadUI32Block(length, ref index);
        }
        public uint[] ReadUI32Block(int length, ref int index)
        {
            uint[] buffer = new uint[length];

            for (int i = 0; i < length; i++)
                buffer[i] = ReadUI32(ref index);

            return buffer;
        }
        #endregion

        public void ReplaceUI8Block(int index, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
                _buffer[index++] = data[i];
        }

        public byte[] ToBytes()
        {
            return _buffer;
        }
        public void Save(string path, bool compress)
        {
            if (compress) Compress();
            File.WriteAllBytes(path, _buffer);
        }

        public byte[] Compile()
        {
            var bodyBuffer = new List<byte>();
            bodyBuffer.AddRange(FrameSize.ToBytes());
            bodyBuffer.AddRange(BitConverter.GetBytes((ushort)(FrameRate << 8)));
            bodyBuffer.AddRange(BitConverter.GetBytes((ushort)FrameCount));

            IList<IFlashTag> tags = Tags;
            foreach (IFlashTag tag in tags) bodyBuffer.AddRange(tag.ToBytes());

            var buffer = new List<byte>(8 + bodyBuffer.Count);
            buffer.AddRange(Encoding.UTF8.GetBytes(FWS));
            buffer.Add((byte)Version);
            buffer.AddRange(BitConverter.GetBytes(buffer.Capacity));
            buffer.AddRange(bodyBuffer);

            _buffer = buffer.ToArray();
            _position = _buffer.Length;

            return _buffer;
        }
        public static byte[] ConstructTagData(FlashTagType tag, byte[] body, bool forceLong = false)
        {
            var header = ((uint)tag) << 6;
            bool isLong = (forceLong || body.Length >= 0x3f);
            header |= (isLong ? 0x3f : (uint)body.Length);

            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((ushort)header));
            if (isLong) buffer.AddRange(BitConverter.GetBytes(body.Length));

            buffer.AddRange(body);

            return buffer.ToArray();
        }
    }
}