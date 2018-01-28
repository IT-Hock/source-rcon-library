using System.Net;
using Xunit;

namespace RCONServerLib.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void TestAuthFail()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();
            
            var client = new RemoteConClient();
            client.OnAuthResult += Assert.False;
            
            client.Connect("127.0.0.1", 27015);
            client.Authenticate("unitfail");
            
            client.Disconnect();
            server.StopListening();
        }
        
        [Fact]
        public void TestAuthSuccess()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();
            
            var client = new RemoteConClient();
            client.OnAuthResult += Assert.True;
            
            client.Connect("127.0.0.1", 27015);
            client.Authenticate("changeme");
            
            client.Disconnect();
            server.StopListening();
        }
        
        [Fact]
        public void TestCommandFail()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();
            
            var client = new RemoteConClient();
            client.OnAuthResult += success =>
            {
                //Assert.True(success);
                client.SendCommand("testing", result =>
                {
                    Assert.Contains("invalid command", result);
                });
            };
            
            client.Connect("127.0.0.1", 27015);
            client.Authenticate("changeme");
            
            client.Disconnect();
            server.StopListening();
        }
        
        [Fact]
        public void TestCommandSuccess()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();
            
            var client = new RemoteConClient();
            client.OnAuthResult += success =>
            {
                //Assert.True(success);
                client.SendCommand("hello", result =>
                {
                    Assert.Contains("world", result);
                });
            };
            
            client.Connect("127.0.0.1", 27015);
            client.Authenticate("changeme");
            
            client.Disconnect();
            server.StopListening();
        }
    }
}