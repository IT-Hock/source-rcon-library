using System;
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
			using(var ms = new MemoryStream(testBytes))
			{
				using(var reader = new BinaryReaderExt(ms))
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
	    public void RemoteConProcessPacketTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => { return "test"; });
		    
		    var client = new RemoteConClient(server);
		    
		    // Auth wrong test
		    client.ProcessPacket(new byte[]
		    {
			    0x15, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x03, 0x00, 0x00, 0x00, // Type
			    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
			    0x00,
		    });
		    
		    // Auth correct test
		    client.ProcessPacket(new byte[]
		    {
			    0x1D, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x03, 0x00, 0x00, 0x00, // Type
			    0x73, 0x75, 0x70, 0x65, 0x72, 0x73, 0x65, 0x63, 0x75, 0x72, 0x65, 0x70, 0x61, 0x73, 0x73, 0x77, 0x6F, 0x72, 0x64, 0x00,
			    0x00,
		    });
		    client.Authenticated = true;
		    
		    // No command found test
		    client.ProcessPacket(new byte[]
		    {
			    0x15, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x02, 0x00, 0x00, 0x00, // Type
			    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
			    0x00,
		    });
		    // Command test
		    client.ProcessPacket(new byte[]
		    {
			    0x0E, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x02, 0x00, 0x00, 0x00, // Type
			    0x74, 0x65, 0x73, 0x74, 0x00,
			    0x00,
		    });
		    // Empty payload test
		    client.ProcessPacket(new byte[]
		    {
			    0x0A, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x02, 0x00, 0x00, 0x00, // Type
			    0x00,
			    0x00,
		    });
		    // Type other than execcommand
		    client.ProcessPacket(new byte[]
		    {
			    0x0E, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x00, 0x00, 0x00, 0x00, // Type
			    0x74, 0x65, 0x73, 0x74, 0x00,
			    0x00,
		    });
	    }
    }
}