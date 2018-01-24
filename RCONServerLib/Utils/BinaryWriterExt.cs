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
    }
}