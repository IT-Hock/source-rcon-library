﻿using System;
using System.IO;
using System.Net;
using RCONServerLib.Utils;
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
        public void RconPacketTypeTest()
        {
            Assert.Throws<InvalidPacketTypeException>(() => new RemoteConPacket(new byte[]
            {
                0x08, 0x00, 0x00, 0x00, // Size
                0x00, 0x00, 0x00, 0x00, // Id
                0xFF, 0xFF, 0x00, 0x00 // Type
            }));
        }

        [Fact]
        public void IpPatternMatchTest()
        {
            Assert.True(IpExtension.Match("127.0.0.1", "127.0.0.1"));
            Assert.True(IpExtension.Match("192.*.*.*", "192.168.178.1"));
            Assert.True(IpExtension.Match("192.*.*.*", "192.255.255.255"));

            Assert.False(IpExtension.Match("127.0.0.1", "127.0.0.2"));
            Assert.False(IpExtension.Match("127.0.0.1", "127.0.0.9"));
            Assert.False(IpExtension.Match("192.*.*.*", "178.75.68.49"));
        }

        [Fact]
        public void RconPacketSuccessTest()
        {
            var packet = new RemoteConPacket(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00, // Size
                0x02, 0x00, 0x00, 0x00, // Id
                0x03, 0x00, 0x00, 0x00, // Type
                0x00,
                0x00,
            });

            Assert.Equal(2, packet.Id);
            Assert.Equal(10, packet.Size);
            Assert.Equal(RemoteConPacket.PacketType.Auth, packet.Type);
        }

        [Fact]
        public void RconPacketGetBytesTest()
        {
            var testBytes = new byte[]
            {
                0x0A, 0x00, 0x00, 0x00, // Size
                0x02, 0x00, 0x00, 0x00, // Id
                0x03, 0x00, 0x00, 0x00, // Type
                0x00,
                0x00,
            };
            var packet = new RemoteConPacket(2, RemoteConPacket.PacketType.Auth, "");

            var packetBytes = packet.GetBytes();
            for (var index = 0; index < testBytes.Length; index++)
                Assert.Equal(testBytes[index], packetBytes[index]);
        }

        [Fact]
        public void ExtendedBinaryReaderTest()
        {
            var testBytes = new byte[]
            {
                0x15, 0x00, 0x00, 0x00, // Size
                0x02, 0x00, 0x00, 0x00, // Id
                0x03, 0x00, 0x00, 0x00, // Type
                0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
                0x00,
            };
            using (var ms = new MemoryStream(testBytes))
            {
                using (var reader = new BinaryReaderExt(ms))
                {
                    var sizeTestValue = reader.ReadInt32LittleEndian();
                    Assert.Equal(21, sizeTestValue);
                    var idTestValue = reader.ReadInt32LittleEndian();
                    Assert.Equal(2, idTestValue);
                    var typeTestValue = reader.ReadInt32LittleEndian();
                    Assert.Equal(3, typeTestValue);
                    var payloadTestValue = reader.ReadAscii();
                    Assert.Equal("Hello World", payloadTestValue);
                }
            }
        }

        [Fact]
        public void DateTimeTest()
        {
            var nowTime = DateTime.Now;
            var unix = nowTime.ToUnixTimestamp();
            Assert.True(nowTime.Day == DateTimeExtensions.FromUnixTimestamp(unix).Day, "Day doesn't match");
            Assert.True(nowTime.Month == DateTimeExtensions.FromUnixTimestamp(unix).Month, "Month doesn't match");
            Assert.True(nowTime.Year == DateTimeExtensions.FromUnixTimestamp(unix).Year, "Year doesn't match");
            Assert.True(nowTime.Hour == DateTimeExtensions.FromUnixTimestamp(unix).Hour, "Hour doesn't match");
            Assert.True(nowTime.Minute == DateTimeExtensions.FromUnixTimestamp(unix).Minute, "Minute doesn't match");
            Assert.True(nowTime.Second == DateTimeExtensions.FromUnixTimestamp(unix).Second, "Second doesn't match");
            
            var unix2 = DateTime.Now.UnixTimestamp();
            Assert.True(nowTime.Day == DateTimeExtensions.FromUnixTimestamp(unix2).Day, "Day doesn't match");
            Assert.True(nowTime.Month == DateTimeExtensions.FromUnixTimestamp(unix2).Month, "Month doesn't match");
            Assert.True(nowTime.Year == DateTimeExtensions.FromUnixTimestamp(unix2).Year, "Year doesn't match");
            Assert.True(nowTime.Hour == DateTimeExtensions.FromUnixTimestamp(unix2).Hour, "Hour doesn't match");
            Assert.True(nowTime.Minute == DateTimeExtensions.FromUnixTimestamp(unix2).Minute, "Minute doesn't match");
            Assert.True(nowTime.Second == DateTimeExtensions.FromUnixTimestamp(unix2).Second, "Second doesn't match");
        }
    }
}