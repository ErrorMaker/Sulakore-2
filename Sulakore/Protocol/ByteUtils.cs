using System;
using System.Collections.Generic;

namespace Sulakore.Protocol
{
    public static class ByteUtils
    {
        private static readonly object _splitLock;

        static ByteUtils()
        {
            _splitLock = new object();
        }

        public static IList<byte[]> Split(ref byte[] cache, byte[] data, bool shouldSplit)
        {
            if (!shouldSplit)
                return new[] { data };

            lock (_splitLock)
            {
                if (cache != null)
                {
                    data = Merge(cache, data);
                    cache = null;
                }

                var chunks = new List<byte[]>();
                int length = BigEndian.DecypherInt(data);
                if (length == data.Length - 4) chunks.Add(data);
                else
                {
                    do
                    {
                        if (length > data.Length - 4) { cache = data; break; }
                        chunks.Add(CutBlock(ref data, 0, length + 4));

                        if (data.Length >= 4)
                            length = BigEndian.DecypherInt(data);
                    }
                    while (data.Length != 0);
                }
                return chunks;
            }
        }

        public static byte[] Merge(byte[] source, params byte[][] chunks)
        {
            var data = new List<byte>();
            data.AddRange(source);
            foreach (byte[] chunk in chunks)
                data.AddRange(chunk);
            return data.ToArray();
        }
        public static byte[] CopyBlock(byte[] data, int offset, int length)
        {
            length = (length > data.Length) ? data.Length : length < 0 ? 0 : length;
            offset = offset + length >= data.Length ? data.Length - length : offset < 0 ? 0 : offset;

            var chunk = new byte[length];
            for (int i = 0; i < length; i++) chunk[i] = data[offset++];
            return chunk;
        }
        public static byte[] CutBlock(ref byte[] data, int offset, int length)
        {
            length = (length > data.Length) ? data.Length : length < 0 ? 0 : length;
            offset = offset + length >= data.Length ? data.Length - length : offset < 0 ? 0 : offset;

            var chunk = new byte[length];
            var trimmed = new byte[data.Length - length];
            for (int i = 0, j = offset; i < length; i++) chunk[i] = data[j++];
            for (int i = 0, j = 0; i < data.Length; i++)
            {
                if (i < offset || i >= offset + length)
                    trimmed[j++] = data[i];
            }
            data = trimmed;
            return chunk;
        }
    }
}