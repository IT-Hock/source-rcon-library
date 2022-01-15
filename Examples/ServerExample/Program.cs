using System;
using System.Net;
using System.Text;
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
                Debug = true
            };
            server.CommandManager.Add("hello", "Echos back world", (command, arguments) => { return "world"; });
            server.CommandManager.Add("help", "(command)", "Shows this help", (cmd, arguments) =>
            {
                if (arguments.Count == 1)
                {
                    var helpCmdStr = arguments[0];
                    var helpCmd = server.CommandManager.GetCommand(helpCmdStr);
                    if (helpCmd == null)
                        return "Command not found.";

                    return string.Format("{0} - {1}", helpCmd.Name, helpCmd.Description);
                }

                var sb = new StringBuilder();

                var all = server.CommandManager.Commands.Count;
                var i = 0;
                foreach (var command in server.CommandManager.Commands)
                {
                    if (command.Value.Usage == "")
                        sb.AppendFormat("{0}", command.Value.Name);
                    else
                        sb.AppendFormat("{0} {1}", command.Value.Name, command.Value.Usage);
                    if (i < all)
                        sb.Append(", ");

                    i++;
                }

                return sb.ToString();
            });

            server.StartListening();

            Console.WriteLine("Server started. Press any key to stop.");
            Console.ReadKey();
        }
    }
}