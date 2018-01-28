using System;
using System.Net;
using RCONServerLib;

namespace ServerExample
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var server = new RemoteConServer(IPAddress.Any, 27015)
            {
                SendAuthImmediately = true,
                Debug = true,
            };
            server.CommandManager.Add("hello", "", (command, arguments) => { return "world"; });

            server.StartListening();
            
            Console.WriteLine("Server started. Press any key to stop.");
            Console.ReadKey();
        }
    }
}