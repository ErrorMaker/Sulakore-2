using System;
using System.Text;
using System.Drawing;

namespace Sulakore.Protocol.Encryption
{
    public class HKeyExchange
    {
        #region Private Fields
        private readonly int _bitSize;
        private static readonly Random ByteGen;
        #endregion

        #region Public Properties
        public RsaKey Rsa { get; private set; }

        public BigInteger DhPrime { get; private set; }
        public BigInteger DhGenerator { get; private set; }
        public BigInteger DhPublic { get; private set; }
        public BigInteger DhPrivate { get; private set; }

        private string _signedPrime;
        public string SignedPrime
        {
            get
            {
                if (!IsInitiator || !string.IsNullOrEmpty(_signedPrime)) return _signedPrime;

                byte[] primeAsBytes = Encoding.Default.GetBytes(DhPrime.ToString(10));
                Rsa.Sign(ref primeAsBytes);

                return (_signedPrime = BytesToHex(primeAsBytes).ToLower());
            }
        }

        private string _signedGenerator;
        public string SignedGenerator
        {
            get
            {
                if (!IsInitiator || !string.IsNullOrEmpty(_signedGenerator)) return _signedGenerator;

                byte[] generatorAsBytes = Encoding.Default.GetBytes(DhGenerator.ToString(10));
                Rsa.Sign(ref generatorAsBytes);

                return (_signedGenerator = BytesToHex(generatorAsBytes).ToLower());
            }
        }

        private string _publicKey;
        public string PublicKey
        {
            get
            {
                if (!string.IsNullOrEmpty(_publicKey)) return _publicKey;

                byte[] publicKeyAsBytes = Encoding.Default.GetBytes(DhPublic.ToString(10));
                if (IsInitiator) Rsa.Sign(ref publicKeyAsBytes);
                else Rsa.Encrypt(ref publicKeyAsBytes);

                return (_publicKey = BytesToHex(publicKeyAsBytes).ToLower());
            }
        }

        public bool IsInitiator { get; private set; }
        public bool IsBannerHandshake { get; private set; }
        #endregion

        #region Constructor(s)
        static HKeyExchange()
        {
            ByteGen = new Random();
        }

        public HKeyExchange(int e, string n, int bitSize = 16)
            : this(e, n, null, bitSize) { }
        public HKeyExchange(int e, string n, string d, int bitSize = 16)
        {
            _bitSize = bitSize;
            IsInitiator = !string.IsNullOrWhiteSpace(d);

            Rsa = IsInitiator
                ? RsaKey.ParsePrivateKey(e, n, d)
                : RsaKey.ParsePublicKey(e, n);

            if (IsInitiator)
            {
                do { DhPrime = BigInteger.GenPseudoPrime(212, 6, ByteGen); }
                while (!DhPrime.IsProbablePrime());

                do { DhGenerator = BigInteger.GenPseudoPrime(212, 6, ByteGen); }
                while (DhGenerator >= DhPrime && !DhPrime.IsProbablePrime());

                if (DhGenerator > DhPrime)
                {
                    BigInteger dhGenShell = DhGenerator;
                    DhGenerator = DhPrime;
                    DhPrime = dhGenShell;
                }

                DhPrivate = new BigInteger(RandomHex(30), bitSize);
                DhPublic = DhGenerator.ModPow(DhPrivate, DhPrime);
            }
        }
        #endregion

        #region Public Methods
        public byte[] GetSharedKey(string publicKey)
        {
            if (!IsBannerHandshake)
            {
                byte[] paddedPublicKeyAsBytes = HexToBytes(publicKey);
                if (IsInitiator) Rsa.Decrypt(ref paddedPublicKeyAsBytes);
                else Rsa.Verify(ref paddedPublicKeyAsBytes);

                publicKey = Encoding.Default.GetString(paddedPublicKeyAsBytes);
            }

            var unpaddedPublicKey = new BigInteger(publicKey, 10);
            return unpaddedPublicKey.ModPow(DhPrivate, DhPrime).ToBytes();
        }
        public void DoHandshake(Bitmap banner, string token)
        {
            IsBannerHandshake = true;
            var bannerData = new byte[banner.Width * banner.Height * 4];
            for (int y = 0, i = 0; y < banner.Height; y++)
            {
                for (int x = 0; x < banner.Width; x++)
                {
                    int pixelArgb = banner.GetPixel(x, y).ToArgb();
                    bannerData[i++] = (byte)((pixelArgb >> 24) & 255);
                    bannerData[i++] = (byte)((pixelArgb >> 16) & 255);
                    bannerData[i++] = (byte)((pixelArgb >> 8) & 255);
                    bannerData[i++] = (byte)(pixelArgb & 255);
                }
            }

            string bannerChunk = Xor(Decode(bannerData), token);
            int bannerSize = bannerChunk[0];
            bannerChunk = bannerChunk.Substring(1);
            DhPrime = new BigInteger(bannerChunk.Substring(0, bannerSize), 10);

            bannerChunk = bannerChunk.Substring(bannerSize);
            bannerSize = bannerChunk[0];
            bannerChunk = bannerChunk.Substring(1);
            DhGenerator = new BigInteger(bannerChunk.Substring(0, bannerSize), 10);

            DhPrivate = new BigInteger(RandomHex(30), _bitSize);
            DhPublic = DhGenerator.ModPow(DhPrivate, DhPrime);
        }
        public void DoHandshake(string signedPrime, string signedGenerator)
        {
            if (IsInitiator) return;

            byte[] signedPrimeAsBytes = HexToBytes(signedPrime);
            Rsa.Verify(ref signedPrimeAsBytes);

            byte[] signedGeneratorAsBytes = HexToBytes(signedGenerator);
            Rsa.Verify(ref signedGeneratorAsBytes);

            DhPrime = new BigInteger(Encoding.Default.GetString(signedPrimeAsBytes), 10);
            DhGenerator = new BigInteger(Encoding.Default.GetString(signedGeneratorAsBytes), 10);

            if (DhPrime <= 2) throw new Exception("Prime cannot be <= 2!\nPrime: " + DhPrime);
            if (DhGenerator >= DhPrime) throw new Exception(string.Format("Generator cannot be >= Prime!\nPrime: {0}\nGenerator: {1}", DhPrime, DhGenerator));

            DhPrivate = new BigInteger(RandomHex(30), _bitSize);
            DhPublic = DhGenerator.ModPow(DhPrivate, DhPrime);
        }

        public void Flush()
        {
            if (!IsInitiator)
                DhPrime = DhGenerator = DhPublic = DhPrivate = null;

            _signedPrime = _signedGenerator = _publicKey = string.Empty;
            IsBannerHandshake = false;
        }
        #endregion

        #region Static Methods
        public static string Decode(byte[] data)
        {
            int l7 = 0, l8 = 0;
            string decoded = string.Empty;
            for (int i = 39; i < 69; i++)
            {
                for (int j = 4, k = 0; j < 84; j++)
                {
                    int position = ((i + k) * 100 + j) * 4;
                    for (int l = 1; l < 4; l++)
                    {
                        l8 |= (data[position + l] & 1) << (7 - l7);
                        if (l7 == 7)
                        {
                            decoded += (char)l8;
                            l7 = l8 = 0;
                        }
                        else l7++;
                    }
                    if (j % 2 == 0) k++;
                }
            }
            return decoded;
        }
        public static byte[] HexToBytes(string hex)
        {
            int hexLength = hex.Length;
            var data = new byte[hexLength / 2];
            for (int i = 0; i < hexLength; i += 2)
                data[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return data;
        }
        public static string BytesToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }
        public static string RandomHex(int length = 16)
        {
            string hex = string.Empty;
            for (int i = 0; i < length; i++)
            {
                var generated = (byte)ByteGen.Next(0, 256);
                hex += Convert.ToString(generated, 16);
            }
            return hex;
        }
        public static string Xor(string value, string token)
        {
            string outcome = string.Empty;
            for (int i = 0, j = 0; i < value.Length; i++)
            {
                outcome += (char)(value[i] ^ token[j]);
                if (++j == token.Length) j = 0;
            }
            return outcome;
        }
        #endregion
    }
}