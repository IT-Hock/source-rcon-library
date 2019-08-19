using System;
using System.Net;
using System.Threading;
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

            bool authResult = false;
            using (var waitEvent = new AutoResetEvent(false))
            {
                var client = new RemoteConClient();
                client.OnAuthResult += success =>
                {
                    authResult = success;

                    client.Disconnect();
                    server.StopListening();
                };

                client.Connect("127.0.0.1", 27015);
                client.Authenticate("unitfail");
            }

            Assert.False(authResult);
        }

        [Fact]
        public void TestAuthSuccess()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();

            bool authResult = false;
            using (var waitEvent = new AutoResetEvent(false))
            {
                var client = new RemoteConClient();
                client.OnAuthResult += success =>
                {
                    authResult = success; 

                    client.Disconnect();
                    server.StopListening();
                    waitEvent.Set();
                };

                client.Connect("127.0.0.1", 27015);
                client.Authenticate("changeme");
                waitEvent.WaitOne();
            }

            Assert.True(authResult);
        }

        [Fact]
        public void TestCommandFail()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();

            string commandResult = null;
            using (var waitEvent = new AutoResetEvent(false))
            {
                var client = new RemoteConClient();
                client.OnAuthResult += success =>
                {
                    client.SendCommand("testing", result =>
                    {
                        commandResult = result;

                        client.Disconnect();
                        server.StopListening();
                        waitEvent.Set();
                    });
                };

                client.Connect("127.0.0.1", 27015);
                client.Authenticate("changeme");
                waitEvent.WaitOne();
            }

            Assert.Contains("invalid command", commandResult);
        }

        [Fact]
        public void TestCommandSuccess()
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();

            string commandResult = null;
            using (var waitEvent = new AutoResetEvent(false))
            {
                var client = new RemoteConClient();
                client.OnAuthResult += success =>
                {
                    client.SendCommand("hello", result =>
                    {
                        commandResult = result;

                        client.Disconnect();
                        server.StopListening();
                        waitEvent.Set();
                    });
                };

                client.Connect("127.0.0.1", 27015);
                client.Authenticate("changeme");
                waitEvent.WaitOne();
            }

            Assert.Contains("world", commandResult);
        }
    }
}