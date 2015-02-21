using System;

/* Copyright
 *
 * RsaKey
 * 
 * Copyright (c) 2014 The Old Nut Man
 * All rights reserved.
 * 
 * Derived From: The jsbn library is Copyright (c) 2003-2005 Tom Wu (tjw@cs.Stanford.EDU)
 * (http://www-cs-students.stanford.edu/~tjw/jsbn/)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
namespace Sulakore.Protocol.Encryption
{
    public class RsaKey
    {
        #region Private Fields
        private static readonly Random ByteGen;
        #endregion

        #region Public Properties
        /// <summary>
        /// Public Exponent
        /// </summary>
        public BigInteger E { get; private set; }

        /// <summary>
        /// Public Modulus
        /// </summary>
        public BigInteger N { get; private set; }

        /// <summary>
        /// Private Exponent
        /// </summary>
        public BigInteger D { get; private set; }

        /// <summary>
        /// Secret Prime Factor ( P )
        /// </summary>
        public BigInteger P { get; private set; }

        /// <summary>
        /// Secret Prime Factor ( Q )
        /// </summary>
        public BigInteger Q { get; private set; }

        /// <summary>
        /// d mod ( p - 1 )
        /// </summary>
        public BigInteger Dmp1 { get; private set; }

        /// <summary>
        /// d mod ( q - 1 )
        /// </summary>
        public BigInteger Dmq1 { get; private set; }

        /// <summary>
        /// (Inverse)q mod p
        /// </summary>
        public BigInteger Iqmp { get; private set; }

        public bool CanEncrypt { get; private set; }
        public bool CanDecrypt { get; private set; }
        #endregion

        #region Constructor(s)
        static RsaKey()
        {
            ByteGen = new Random();
        }
        public RsaKey(BigInteger e, BigInteger n) : this(e, n, null, null, null, null, null, null) { }
        public RsaKey(BigInteger e, BigInteger n, BigInteger d) : this(e, n, d, null, null, null, null, null) { }
        public RsaKey(BigInteger e, BigInteger n, BigInteger d, BigInteger p, BigInteger q, BigInteger dmp1, BigInteger dmq1, BigInteger iqmp)
        {
            E = e;
            N = n;
            D = d;
            P = p;
            Q = q;
            Dmp1 = dmp1;
            Dmq1 = dmq1;
            Iqmp = iqmp;

            CanEncrypt = (e != null && n != null);
            CanDecrypt = (CanEncrypt && d != null);
        }
        #endregion

        #region Public Static Methods
        public static RsaKey Generate(int exponent, int bitSize)
        {
            BigInteger p, q, e = new BigInteger(exponent.ToString(), 16);

            BigInteger phi, p1, q1;
            int qs = bitSize >> 1;
            do
            {
                do p = BigInteger.GenPseudoPrime(bitSize - qs, 6, ByteGen);
                while ((p - 1).Gcd(e) != 1 && !p.IsProbablePrime(10));

                do q = BigInteger.GenPseudoPrime(qs, 6, ByteGen);
                while ((q - 1).Gcd(e) != 1 && !q.IsProbablePrime(10) && q == p);

                if (p < q)
                {
                    BigInteger tmpP = p;
                    p = q; q = tmpP;
                }
                phi = (p1 = (p - 1)) * (q1 = (q - 1));
            }
            while (phi.Gcd(e) != 1);

            BigInteger n = p * q;
            BigInteger d = e.ModInverse(phi);
            BigInteger dmp1 = d % p1;
            BigInteger dmq1 = d % q1;
            BigInteger iqmp = q.ModInverse(p);
            return new RsaKey(e, n, d, p, q, dmp1, dmq1, iqmp);
        }

        public static RsaKey ParsePublicKey(int e, string n)
        {
            return new RsaKey(new BigInteger(e.ToString(), 16), new BigInteger(n, 16));
        }
        public static RsaKey ParsePrivateKey(int e, string n, string d)
        {
            return new RsaKey(new BigInteger(e.ToString(), 16), new BigInteger(n, 16), new BigInteger(d, 16));
        }
        public static RsaKey ParsePrivateKey(int e, string n, string d, string p, string q, string dmp1, string dmq1, string iqmp)
        {
            return new RsaKey(new BigInteger(e.ToString(), 16), new BigInteger(n, 16), new BigInteger(d, 16), new BigInteger(p, 16),
                new BigInteger(q, 16), new BigInteger(dmp1, 16), new BigInteger(dmq1, 16), new BigInteger(iqmp, 16));
        }

        public static byte[] Pkcs1Pad(byte[] data, int length, PKCS1PadType type)
        {
            var outCome = new byte[length];

            for (int i = data.Length - 1; (i >= 0 && length > 11); )
                outCome[--length] = data[i--];

            outCome[--length] = 0;
            while (length > 2)
            {
                byte x = (type == PKCS1PadType.RandomByte) ? (byte)ByteGen.Next(1, 256) : byte.MaxValue;
                outCome[--length] = x;
            }
            outCome[--length] = (byte)(type + 1);
            outCome[--length] = 0;
            return outCome;
        }
        public static byte[] Pkcs1Unpad(byte[] data, int length, PKCS1PadType type)
        {
            int offset = 0;
            while (offset < data.Length && data[offset] == 0) ++offset;
            if (data.Length - offset != length - 1 || data[offset] != ((byte)type + 1))
                throw new Exception(string.Format("PKCS#1 UNPAD: Offset={0}, expected Data[Offset]==[{1}]; Got Data[Offset]={2}", offset, type + 1, data[offset]));

            ++offset;
            while (data[offset] != 0)
            {
                if (++offset >= data.Length)
                    throw new Exception(string.Format("PKCS#1 UNPAD: Offset={0}, Data[Offset - 1] != 0 (={1:X})", offset, data[offset - 1]));
            }

            var outCome = new byte[(data.Length - offset) - 1];
            for (int j = 0; ++offset < data.Length; j++)
                outCome[j] = data[offset];

            return outCome;
        }
        #endregion

        #region Public Methods
        public void Encrypt(ref byte[] data)
        {
            _Encrypt(DoPublic, ref data, PKCS1PadType.RandomByte);
        }
        public void Decrypt(ref byte[] data)
        {
            _Decrypt(DoPrivate, ref data, PKCS1PadType.RandomByte);
        }

        public void Sign(ref byte[] data)
        {
            _Encrypt(DoPrivate, ref data, PKCS1PadType.MaxByte);
        }
        public void Verify(ref byte[] data)
        {
            _Decrypt(DoPublic, ref data, PKCS1PadType.MaxByte);
        }

        public int GetBlockSize()
        {
            return (N.BitCount() + 7) / 8;
        }
        #endregion

        #region Private Methods
        private void _Decrypt(Func<BigInteger, BigInteger> doFunc, ref byte[] data, PKCS1PadType type)
        {
            int blockSize = GetBlockSize();
            data = Pkcs1Unpad(doFunc(new BigInteger(data)).ToBytes(), blockSize, type);
        }
        private void _Encrypt(Func<BigInteger, BigInteger> doFunc, ref byte[] data, PKCS1PadType type)
        {
            int blockSize = GetBlockSize();
            data = doFunc(new BigInteger(Pkcs1Pad(data, blockSize, type))).ToBytes();
        }

        private BigInteger DoPublic(BigInteger x)
        {
            return x.ModPow(E, N);
        }
        private BigInteger DoPrivate(BigInteger x)
        {
            if (P == null && Q == null)
                return x.ModPow(D, N);

            BigInteger xp = (x % P).ModPow(Dmp1, P);
            BigInteger xq = (x % Q).ModPow(Dmq1, Q);

            while (xp < xq) xp = xp + P;
            return ((((xp - xq) * (Iqmp)) % P) * Q) + xq;
        }
        #endregion
    }
}