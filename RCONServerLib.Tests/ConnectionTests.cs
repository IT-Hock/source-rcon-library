using System.Net;
using System.Threading;
using Xunit;

namespace RCONServerLib.Tests
{
    public class ConnectionTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestAuthFail(bool useUtf8)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015)
            {
                UseUtf8 = useUtf8
            };
            server.StartListening();

            var authResult = true;
            var waitEvent = new AutoResetEvent(false);
            var client = new RemoteConClient
            {
                UseUtf8 = useUtf8
            };

            client.OnAuthResult += success =>
            {
                authResult = success;

                client.Disconnect();
                server.StopListening();
                waitEvent.Set();
            };

            client.Connect("127.0.0.1", 27015);
            client.Authenticate("unitfail");
            waitEvent.WaitOne();

            Assert.False(authResult);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestAuthSuccess(bool useUtf8)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015)
            {
                UseUtf8 = useUtf8
            };
            server.StartListening();

            var authResult = false;
            var waitEvent = new AutoResetEvent(false);
            var client = new RemoteConClient
            {
                UseUtf8 = useUtf8
            };
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

            Assert.True(authResult);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestCommandFail(bool useUtf8)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.StartListening();
            server.UseUtf8 = useUtf8;

            string commandResult = null;
            var waitEvent = new AutoResetEvent(false);
            var client = new RemoteConClient
            {
                UseUtf8 = useUtf8
            };
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

            Assert.Contains("Invalid command", commandResult);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestCommandSuccess(bool useUtf8)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015);
            server.CommandManager.Add("hello", "Replies with world", (command, args) => "world");
            server.UseUtf8 = true;
            server.StartListening();

            string commandResult = null;
            var waitEvent = new AutoResetEvent(false);
            var client = new RemoteConClient
            {
                UseUtf8 = useUtf8
            };
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

            Assert.Contains("world", commandResult);
        }
    }
}