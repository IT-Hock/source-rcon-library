using System.Net;
using RCONServerLib;

namespace RCONServer
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015) {SendAuthImmediately = true};
            server.CommandManager.Add("hello", "", (command, arguments) => { return "world"; });
            
            server.StartListening();
            while (true)
            {
            }

            return 0;
        }
    }
}