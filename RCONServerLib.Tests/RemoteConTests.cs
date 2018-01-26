using System.Net;
using RCONServerLib.Utils;
using Xunit;

namespace RCONServerLib.Tests
{
    public class RemoteConTests
    {
	    [Fact]
	    public void RemoteConInvalidAuthPacketTypeTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    
		    // Wrong Auth packet type test
		    Assert.Throws<NotAuthenticatedException>(() =>
		    {
			    client.ParsePacket(new byte[]
			    {
				    0x15, 0x00, 0x00, 0x00, // Size
				    0x02, 0x00, 0x00, 0x00, // Id
				    0x02, 0x00, 0x00, 0x00, // Type
				    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
				    0x00,
			    });
		    });
	    }
	    
	    [Fact]
	    public void RemoteConAuthFailureTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    
		    // Auth wrong test
		    client.ParsePacket(new byte[]
		    {
			    0x15, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x03, 0x00, 0x00, 0x00, // Type
			    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
			    0x00,
		    });
	    }

	    [Fact]
	    public void RemoteConAuthSuccessTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015)
		    {
			    Password = "supersecretpassword"
		    };
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    
		    // Auth correct test
		    client.ParsePacket(new byte[]
		    {
			    0x1D, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x03, 0x00, 0x00, 0x00, // Type
			    0x73, 0x75, 0x70, 0x65, 0x72, 0x73, 0x65, 0x63, 0x75, 0x72, 0x65, 0x70, 0x61, 0x73, 0x73, 0x77, 0x6F, 0x72, 0x64, 0x00,
			    0x00,
		    });
	    }

	    [Fact]
	    public void RemoteConInvalidCommandTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    client.Authenticated = true;
		    
		    // No command found test
		    client.ParsePacket(new byte[]
		    {
			    0x15, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x02, 0x00, 0x00, 0x00, // Type
			    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x00,
			    0x00,
		    });
	    }

	    [Fact]
	    public void RemoteConCommandTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    client.Authenticated = true;
		    
		    // Command test
		    client.ParsePacket(new byte[]
		    {
			    0x0E, 0x00, 0x00, 0x00, // Size
			    0x02, 0x00, 0x00, 0x00, // Id
			    0x02, 0x00, 0x00, 0x00, // Type
			    0x74, 0x65, 0x73, 0x74, 0x00,
			    0x00,
		    });
	    }

	    [Fact]
	    public void RemoteConEmptyPayloadTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    client.Authenticated = true;
		    
		    // Empty payload test
		    Assert.Throws<EmptyPacketPayloadException>(() =>
		    {
			    client.ParsePacket(new byte[]
			    {
				    0x0A, 0x00, 0x00, 0x00, // Size
				    0x02, 0x00, 0x00, 0x00, // Id
				    0x02, 0x00, 0x00, 0x00, // Type
				    0x00,
				    0x00,
			    });			    
		    });
	    }
	    
	    [Fact]
	    public void RemoteConInvalidPacketTypeTest()
	    {
		    var server = new RemoteConServer(IPAddress.Any, 27015);
		    server.CommandManager.Add("test", "Test", (command, args) => "test");
		    
		    var client = new RemoteConTcpClient(server);
		    client.Authenticated = true;
		    
		    // Type other than execcommand
		    Assert.Throws<InvalidPacketTypeException>(() =>
		    {
			    client.ParsePacket(new byte[]
			    {
				    0x0E, 0x00, 0x00, 0x00, // Size
				    0x02, 0x00, 0x00, 0x00, // Id
				    0x00, 0x00, 0x00, 0x00, // Type
				    0x74, 0x65, 0x73, 0x74, 0x00,
				    0x00,
			    });
		    });
	    }
    }
}