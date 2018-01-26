using System;
using RCONServerLib;

namespace RCONServerLibClientExample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var client = new RemoteConClient();
            client.Connect("127.0.0.1", 27015);
            client.Authenticate("changeme");
            while (true)
            {
                var cmd = Console.ReadLine();
                client.SendCommand(cmd, result => Console.WriteLine("CMD Result:\n\t" + result));
            }
        }
    }
}