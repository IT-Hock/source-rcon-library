using System;
using Xunit;

namespace RCONServerLib.Tests
{
    public class Tests
    {
        [Fact]
        public void RconPacketLengthTest()
        {
            Assert.Throws<LengthMismatchException>(() => new RemoteConPacket(new byte[] {0xFF, 0xFF, 0x00, 0x00}));
        }

        [Fact]
        public void RconPacketNullTerminatorEndTest()
        {
            Assert.Throws<NullTerminatorMissingException>(() => new RemoteConPacket(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00, // Size
                0x00, 0x00, 0x00, 0x00, // Id
                0x00, 0x00, 0x00, 0x00, // Type
                0x00, // Payload
                0x01 // Null-terminator end
            }));
        }

        [Fact]
        public void RconPacketTooLongTest()
        {
            var bytes = new byte[4096];
            Array.Copy(new byte[]
            {
                0xFC, 0x0F, 0x00, 0x00, // Size - 4092
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00
            }, 0, bytes, 0, 12);
            bytes[4094] = 0x00;
            bytes[4095] = 0x00;
            for (var i = 13; i < 4094; i++) bytes[i] = 0xFF;
            Assert.Throws<NullTerminatorMissingException>(() => new RemoteConPacket(bytes));
        }

        [Fact]
        public void RconPacketTypeTest()
        {
            Assert.Throws<InvalidPacketTypeException>(() => new RemoteConPacket(new byte[]
            {
                0x08, 0x00, 0x00, 0x00, // Size
                0x00, 0x00, 0x00, 0x00, // Id
                0xFF, 0xFF, 0x00, 0x00 // Type
            }));
        }
    }
}