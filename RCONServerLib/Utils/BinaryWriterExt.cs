using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RCONServerLib.Utils
{
    internal class BinaryWriterExt : BinaryWriter
    {
        public BinaryWriterExt(Stream stream)
            : base(stream, Encoding.Unicode)
        {
        }

        public void WriteLittleEndian(int val)
        {
            var bytes = BitConverter.GetBytes(val);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Write(bytes);
        }

        public static string HexDump(IEnumerable<byte> buffer)
        {
            const int bytesPerLine = 16;
            var hexDump = "";
            var j = 0;
            foreach (var g in buffer.Select((c, i) => new {Char = c, Chunk = i / bytesPerLine}).GroupBy(c => c.Chunk))
            {
                var s1 = g.Select(c => $"{c.Char:X2} ").Aggregate((s, i) => s + i);
                string s2 = null;
                var first = true;
                foreach (var c in g)
                {
                    var s = $"{(c.Char < 32 || c.Char > 122 ? '·' : (char) c.Char)} ";
                    if (first)
                    {
                        first = false;
                        s2 = s;
                        continue;
                    }
                    s2 = s2 + s;
                }
                var s3 = $"{j++ * bytesPerLine:d6}: {s1} {s2}";
                hexDump = hexDump + s3 + Environment.NewLine;
            }
            return hexDump;
        }
    }
}