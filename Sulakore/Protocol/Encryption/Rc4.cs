namespace Sulakore.Protocol.Encryption
{
    public class Rc4
    {
        #region Private Fields
        private int _i, _j;
        private int[] _table;
        #endregion

        #region Constructor(s)
        public Rc4(int key)
        {
            key = key < 0 ? 0 : key;
            var nKey = new[] { (byte)key };
            if (key > byte.MaxValue)
                nKey = key > ushort.MaxValue ? BigEndian.CypherInt(key) : BigEndian.CypherShort((ushort)key);
            Initialize(nKey);
        }
        public Rc4(byte[] key)
        {
            Initialize(key);
        }
        #endregion

        #region Private Methods
        private void Initialize(byte[] key)
        {
            _table = new int[256];
            for (int k = 0; k < 256; k++) _table[k] = k;
            for (int k = 0, enX = 0; k < 256; k++)
                Swap(k, enX = (((enX + _table[k]) + (key[k % key.Length])) % 256));
        }
        private void Swap(int a, int b)
        {
            int save = _table[a];
            _table[a] = _table[b];
            _table[b] = save;
        }
        #endregion

        #region Public Methods
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
            var shallowCopy = new byte[data.Length];
            data.CopyTo(shallowCopy, 0);
            Parse(shallowCopy);
            return shallowCopy;
        }
        #endregion
    }
}