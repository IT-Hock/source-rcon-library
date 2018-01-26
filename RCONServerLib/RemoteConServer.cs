using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RCONServerLib.Utils;

namespace RCONServerLib
{
    public class RemoteConServer
    {
        public delegate string CommandEventHandler(string command, IList<string> args);

        private readonly TcpListener _listener;

        public readonly CommandManager CommandManager = new CommandManager();

        public RemoteConServer(IPAddress bindAddress, int port)
        {
            EmptyPayloadKick = true;
            EnableIpWhitelist = true;
            InvalidPacketKick = true;
            IpWhitelist = new[]
            {
                "127.0.0.1",
                "192.*.*.*"
            };
            MaxPasswordTries = 3;
            Password = "changeme";
            SendAuthImmediately = false;

#if DEBUG
            Debug = true;
#else
            Debug = false;
#endif

            _listener = new TcpListener(bindAddress, port);
        }

        /// <summary>
        ///     When true closes the connection if the payload of the packet is empty.
        ///     Default: True
        /// </summary>
        public bool EmptyPayloadKick { get; set; }

        /// <summary>
        ///     Wether or not to match incoming ips to our whitelist
        ///     <see cref="IpWhitelist" />
        ///     Default: True
        /// </summary>
        public bool EnableIpWhitelist { get; set; }

        /// <summary>
        ///     When true closes the connection if the packet is not of type "ExecCommand"
        ///     Default: True
        /// </summary>
        public bool InvalidPacketKick { get; set; }

        /// <summary>
        ///     An array containing IP Patterns to allow connecting
        ///     eg.
        ///     192.*.*.* matches all ips starting with 192
        ///     127.0.0.* matches all ips starting with 127.0.0.*
        ///     Default: 127.0.0.1, 192.*.*.*
        /// </summary>
        public string[] IpWhitelist { get; set; }

        /// <summary>
        ///     How many failed password attempts a client has before he gets kicked
        ///     (Setting this to zero means close connection after first try)
        ///     Default: 3
        /// </summary>
        public uint MaxPasswordTries { get; set; }

        /// <summary>
        ///     The password to access RCON
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     This provides a fix for "stupid" written clients,
        ///     which do not expect a RESPONSE_VALUE packet before the auth answer
        ///     Default: False
        /// </summary>
        public bool SendAuthImmediately { get; set; }

        /// <summary>
        ///     Wether or not we should invoke <see cref="OnCommandReceived" /> instead of internally parsing the command
        /// </summary>
        public bool UseCustomCommandHandler { get; set; }

        /// <summary>
        ///     Event Handler to parse custom commands
        /// </summary>
        public event CommandEventHandler OnCommandReceived;

        public bool Debug { get; set; }

        /// <summary>
        ///     Starts the TCPListener and begins accepting clients
        /// </summary>
        public void StartListening()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(OnAccept, _listener);
            LogDebug("Started listening on " + ((IPEndPoint)_listener.LocalEndpoint).Address + ", Password is: \"" + Password + "\"");
        }

        public void StopListening()
        {
            _listener.Stop();
        }

        private void OnAccept(IAsyncResult result)
        {
            var tcpClient = _listener.EndAcceptTcpClient(result);

            if (EnableIpWhitelist)
                if (!IpWhitelist.Any(p =>
                    IpExtension.Match(p, ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.ToString())))
                {
                    tcpClient.Close();
                    return;
                }

            LogDebug("Accepted new connection from " + ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address);
            var client = new RemoteConTcpClient(tcpClient, this);

            _listener.BeginAcceptTcpClient(OnAccept, _listener);
        }

        internal string ExecuteCustomCommandHandler(string cmd, IList<string> args)
        {
            return UseCustomCommandHandler && OnCommandReceived != null ? OnCommandReceived(cmd, args) : "";
        }

        internal void LogDebug(string message)
        {
            if (!Debug)
                return;

            System.Diagnostics.Debug.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}