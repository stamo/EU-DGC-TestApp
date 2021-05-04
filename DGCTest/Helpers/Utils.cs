using System;
using System.Collections.Generic;
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

        public static byte[] ConvertConcatToDer(byte[] concat)
        {
            int len = concat.Length / 2;

            byte[] r = new byte[len];
            Array.Copy(concat, 0, r, 0, len);
            r = UnsignedInteger(r);

            byte[] s = new byte[len];
            Array.Copy(concat, len, s, 0, len);
            s = UnsignedInteger(s);

            var x = new List<byte[]>();
            x.Add(new byte[] { 0x30 });
            x.Add(new byte[] { (byte)(r.Length + s.Length) });
            x.Add(r);
            x.Add(s);

            var der = x.SelectMany(p => p).ToArray();
            return der;
        }

        private static byte[] UnsignedInteger(byte[] i)
        {
            int pad = 0, offset = 0;

            while (offset < i.Length && i[offset] == 0)
            {
                offset++;
            }

            if (offset == i.Length)
            {
                return new byte[] { 0x02, 0x01, 0x00 };
            }
            if ((i[offset] & 0x80) != 0)
            {
                pad++;
            }

            int length = i.Length - offset;
            byte[] der = new byte[2 + length + pad];
            der[0] = 0x02;
            der[1] = (byte)(length + pad);
            Array.Copy(i, offset, der, 2 + pad, length);

            return der;
        }

        public static byte[] ConvertDerToConcat(byte[] der, int len)
        {
            // this is far too naive
            byte[] concat = new byte[len * 2];

            // assumes SEQUENCE is organized as "R + S"
            int kLen = 4;
            if (der[0] != 0x30)
            {
                throw new Exception("Unexpected signature input");
            }
            if ((der[1] & 0x80) != 0)
            {
                // offset actually 4 + (7-bits of byte 1)
                kLen = 4 + (der[1] & 0x7f);
            }

            // calculate start/end of R
            int rOff = kLen;
            int rLen = der[rOff - 1];
            int rPad = 0;
            if (rLen > len)
            {
                rOff += (rLen - len);
                rLen = len;
            }
            else
            {
                rPad = (len - rLen);
            }
            // copy R
            Array.Copy(der, rOff, concat, rPad, rLen);

            // calculate start/end of S
            int sOff = rOff + rLen + 2;
            int sLen = der[sOff - 1];
            int sPad = 0;
            if (sLen > len)
            {
                sOff += (sLen - len);
                sLen = len;
            }
            else
            {
                sPad = (len - sLen);
            }
            // copy S
            Array.Copy(der, sOff, concat, len + sPad, sLen);

            return concat;
        }
    }
}
