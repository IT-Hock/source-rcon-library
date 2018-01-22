using System;
using System.IO;
using System.Text;

namespace RCONServerLib.Utils
{
    internal class BinaryReaderExt : BinaryReader
    {
        public BinaryReaderExt(Stream stream)
            : base(stream, Encoding.Unicode)
        {
        }

        public int ReadInt32LittleEndian()
        {
            var intBytes = ReadBytes(4);
            var bytes = new byte[4];
            Array.Copy(intBytes, 0, bytes, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }

        public string ReadAscii()
        {
            var sb = new StringBuilder();
            byte val;
            do
            {
                val = ReadByte();
                if (val > 0)
                    sb.Append((char)val);
            } while (val > 0);
            return sb.ToString();
        }
    }
}