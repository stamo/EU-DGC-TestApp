using System;
using System.Linq;
using System.Text;

namespace DGCTest
{
    /// <summary>
    /// General Utils
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Encodes byte array to HEX string
        /// </summary>
        /// <param name="bytes">Byte array to be encoded</param>
        /// <returns>HEX encoded string</returns>
        public static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes HEX encoded string to byte array
        /// </summary>
        /// <param name="hex">HEX encoded string</param>
        /// <returns>Decoded byte array</returns>
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
