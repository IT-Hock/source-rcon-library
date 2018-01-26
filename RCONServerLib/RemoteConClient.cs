using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using RCONServerLib.Utils;

namespace RCONServerLib
{
    public class RemoteConClient
    {
        public delegate void CommandResult(string result);

        private const int MaxAllowedPacketSize = 4096;
        private TcpClient _client;
        private NetworkStream _ns;

        private bool _authenticated;

        private byte[] _buffer;

        private int _packetId;

        private Dictionary<int, CommandResult> _requestedCommands;

        public RemoteConClient()
        {
            _client = new TcpClient();

            _packetId = 0;
            _requestedCommands = new Dictionary<int, CommandResult>();
        }

        public void Connect(string hostname, int port)
        {
            _client.Connect(hostname, port);
            if (!_client.Connected) return;
            _ns = _client.GetStream();

            // As indicated by specification the maximum packet size is 4096
            // NOTE: Not sure if only the server is allowed to sent packets with max 4096 or both parties!
            _buffer = new byte[MaxAllowedPacketSize];
            _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
        }

        public void Authenticate(string password)
        {
            _packetId++;
            var packet = new RemoteConPacket(_packetId, RemoteConPacket.PacketType.Auth, password);
            SendPacket(packet);
        }

        public void SendCommand(string command, CommandResult resultFunc)
        {
            if (!_client.Connected)
                return;

            if (!_authenticated)
                throw new NotAuthenticatedException();

            _packetId++;
            _requestedCommands.Add(_packetId, resultFunc);

            var packet = new RemoteConPacket(_packetId, RemoteConPacket.PacketType.ExecCommand, command);
            SendPacket(packet);
        }

        /// <summary>
        ///     Sends the specified packet to the client
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <exception cref="Exception">Not connected</exception>
        private void SendPacket(RemoteConPacket packet)
        {
            if (!_client.Connected)
                throw new Exception("Not connected.");

            var packetBytes = packet.GetBytes();
            //_ns.Write(packetBytes, 0, packetBytes.Length);
            _ns.BeginWrite(packetBytes, 0, packetBytes.Length - 1, ar => { _ns.EndWrite(ar); }, null);
        }

        /// <summary>
        ///     Handles packets
        /// </summary>
        /// <param name="result"></param>
        private void OnPacket(IAsyncResult result)
        {
            var bytesRead = _ns.EndRead(result);
            if (!_client.Connected)
                return;

            if (bytesRead == 0)
            {
                _buffer = new byte[MaxAllowedPacketSize];
                _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
                return;
            }

            Array.Resize(ref _buffer, bytesRead);

            ParsePacket(_buffer);

            if (!_client.Connected)
                return;

            _buffer = new byte[MaxAllowedPacketSize];
            _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
        }

        /// <summary>
        ///     Parses raw bytes to RemoteConPacket
        /// </summary>
        /// <param name="rawPacket"></param>
        internal void ParsePacket(byte[] rawPacket)
        {
            try
            {
                var packet = new RemoteConPacket(rawPacket);
                if (!_authenticated)
                {
                    // ExecCommand is AuthResponse too.
                    if (packet.Type == RemoteConPacket.PacketType.ExecCommand)
                    {
                        if (packet.Id == -1)
                        {
                            _authenticated = false;
                            Debug.WriteLine("Auth failed.");
                        }
                        else
                        {
                            _authenticated = true;
                        }
                    }

                    return;
                }

                if (_requestedCommands.ContainsKey(packet.Id) &&
                    packet.Type == RemoteConPacket.PacketType.ResponseValue)
                    _requestedCommands[packet.Id](packet.Payload);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}