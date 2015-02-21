namespace Sulakore.Protocol.Encryption
{
    public enum PKCS1PadType
    {
        /// <summary>
        /// Represents a padding type that will attempt to fill a byte array with the maximuze value of a System.Byte(byte.MaxValue = 255).
        /// </summary>
        MaxByte = 0,
        /// <summary>
        /// Represents a padding type that will attempt to fill a byte array with random bytes in the range of 1 - 255.
        /// </summary>
        RandomByte = 1
    }
}