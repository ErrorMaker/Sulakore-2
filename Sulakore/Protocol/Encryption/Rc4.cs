using System;

namespace Sulakore.Protocol.Encryption
{
    public class Rc4
    {
        private int _i, _j;
        private readonly int[] _table;

        public Rc4(byte[] key)
        {
            _table = new int[256];

            for (int i = 0; i < 256; i++)
                _table[i] = i;

            for (int j = 0, enX = 0; j < 256; j++)
                Swap(j, enX = (((enX + _table[j]) + (key[j % key.Length])) % 256));
        }

        public void Parse(byte[] data)
        {
            for (int k = 0; k < data.Length; k++)
            {
                Swap(_i = (++_i % 256), _j = ((_j + _table[_i]) % 256));
                data[k] ^= (byte)(_table[(_table[_i] + _table[_j]) % 256]);
            }
        }
        public byte[] SafeParse(byte[] data)
        {
            var dataCopy = new byte[data.Length];

            Buffer.BlockCopy(data, 0, dataCopy, 0, data.Length);
            Parse(dataCopy);

            return dataCopy;
        }

        private void Swap(int a, int b)
        {
            int temp = _table[a];
            _table[a] = _table[b];
            _table[b] = temp;
        }
    }
}