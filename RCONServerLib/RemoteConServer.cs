using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using RCONServerLib.Utils;

namespace RCONServerLib
{
    public class RemoteConServer
    {
        private readonly TcpListener _listener;

        /// <summary>
        /// The password to access RCON
        /// </summary>
        public string Password = "supersecurepassword";
        
        /// <summary>
        /// How many failed password attempts a client has before he gets kicked
        /// (Setting this to zero means close connection after first try)
        /// </summary>
        public uint MaxPasswordTries = 3;

        /// <summary>
        /// This provides a fix for "stupid" written clients,
        /// which do not expect a RESPONSE_VALUE packet before the auth answer
        /// </summary>
        public bool SendAuthImmediately = false;

        /// <summary>
        /// When true closes the connection if the packet is not of type "ExecCommand"
        /// </summary>
        public bool InvalidPacketKick = true;
        
        /// <summary>
        /// When true closes the connection if the payload of the packet is empty.
        /// </summary>
        public bool EmptyPayloadKick = true;

        /// <summary>
        /// Wether or not to match incoming ips to our whitelist
        /// <see cref="IpWhitelist"/>
        /// </summary>
        public bool EnableIpWhitelist = true;

        /// <summary>
        /// An array containing IP Patterns
        /// eg.
        ///     192.*.*.* matches all ips starting with 192
        ///     127.0.0.* matches all ips starting with 127.0.0.*
        /// </summary>
        public string[] IpWhitelist = new[]
        {
            "127.0.0.1",
            "192.*.*.*"
        };

        public readonly CommandManager CommandManager = new CommandManager();

        public RemoteConServer(IPAddress bindAddress, int port)
        {
            _listener = new TcpListener(bindAddress, port);
        }

        public void StartListening()
        {
            _listener.Start();
            _listener.BeginAcceptTcpClient(OnAccept, _listener);
        }

        private void OnAccept(IAsyncResult result)
        {
            var tcpClient = _listener.EndAcceptTcpClient(result);

            if (EnableIpWhitelist)
            {
                if (!IpWhitelist.Any(p =>
                    IpExtension.Match(p, ((IPEndPoint) tcpClient.Client.RemoteEndPoint).Address.ToString())))
                {
                    tcpClient.Close();
                    return;
                }
            }

            var client = new RemoteConClient(tcpClient, this);

            _listener.BeginAcceptTcpClient(OnAccept, _listener);
        }
    }
}