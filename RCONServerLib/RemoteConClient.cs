using System;
using System.Net.Sockets;
using RCONServerLib.Utils;

namespace RCONServerLib
{
    /// <summary>
    ///     Client connected to the remote console
    /// </summary>
    internal class RemoteConClient
    {
        private const int MaxAllowedPacketSize = 4096;
        private readonly NetworkStream _ns;
        private readonly RemoteConServer _remoteConServer;
        private readonly TcpClient _tcp;
        private bool _authenticated;
        private int _authTries;

        private byte[] _buffer;

        private bool _connected;

        public RemoteConClient(TcpClient tcp, RemoteConServer remoteConServer)
        {
            _tcp = tcp;
            _remoteConServer = remoteConServer;

            _ns = tcp.GetStream();
            _connected = true;
            _authenticated = false;

            try
            {
                // Connection was closed.
                if (!_tcp.Connected)
                    return;

                // As indicated by specification the maximum packet size is 4096
                // NOTE: Not sure if only the server is allowed to sent packets with max 4096 or both parties!
                _buffer = new byte[MaxAllowedPacketSize];
                _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        ///     Closes the connection with the client
        /// </summary>
        private void CloseConnection()
        {
            _connected = false;

            if (!_tcp.Connected)
                return;

            _tcp.Client.Disconnect(false);
            _tcp.Close();
        }

        /// <summary>
        ///     Sends the specified packet to the client
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <exception cref="Exception">Not connected</exception>
        private void SendPacket(RemoteConPacket packet)
        {
            if (!_connected)
                throw new Exception("Not connected.");

            var ackBytes = packet.GetBytes();
            _ns.Write(ackBytes, 0, ackBytes.Length);
        }

        /// <summary>
        ///     Handles packets
        /// </summary>
        /// <param name="result"></param>
        private void OnPacket(IAsyncResult result)
        {
            try
            {
                var bytesRead = _ns.EndRead(result);
                if (!_connected)
                {
                    CloseConnection();
                    return;
                }

                if (bytesRead == 0)
                {
                    _buffer = new byte[MaxAllowedPacketSize];
                    _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
                    return;
                }

                if (_buffer[_buffer.Length - 1] != 0x0 || _buffer[_buffer.Length - 2] != 0x0)
                {
#if DEBUG
                    Console.WriteLine("Packet missing null-terminators!");
#else
                    CloseConnection();
#endif
                    return;
                }

                // Resize buffer to actual amount read.
                Array.Resize(ref _buffer, bytesRead);
                /*var buffer = new byte[bytesRead];
                Array.Copy(_buffer, 0, buffer, 0, bytesRead);*/

                var packet = new RemoteConPacket(_buffer);

                // Do not allow any other packets than auth to be sent when client is not authenticated
                if (!_authenticated)
                {
                    if (packet.Type != RemoteConPacket.PacketType.Auth)
                        CloseConnection();

                    _authTries++;

                    if (packet.Payload == _remoteConServer.Password)
                    {
                        _authenticated = true;

                        if (!_remoteConServer.SendAuthImmediately)
                            SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue, ""));

                        SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ExecCommand, ""));
                    }
                    else
                    {
                        if (_authTries >= _remoteConServer.MaxPasswordTries)
                        {
                            CloseConnection();
                            return;
                        }

                        if (!_remoteConServer.SendAuthImmediately)
                            SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue, ""));

                        SendPacket(new RemoteConPacket(-1, RemoteConPacket.PacketType.ExecCommand, ""));
                    }
                }
                else
                {
                    // Invalid packet type.
                    if (packet.Type != RemoteConPacket.PacketType.ExecCommand)
                    {
                        if (_remoteConServer.InvalidPacketKick)
                            CloseConnection();
                        return;
                    }

                    if (packet.Payload == "")
                    {
                        if (_remoteConServer.EmptyPayloadKick)
                            CloseConnection();
                        return;
                    }

                    var args = ArgumentParser.ParseLine(packet.Payload);
                    var cmd = args[0];
                    args.RemoveAt(0);
                    var command = _remoteConServer.CommandManager.GetCommand(cmd);
                    if (command == null)
                    {
                        SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue,
                            "Invalid command \"" + packet.Payload + "\""));
                    }
                    else
                    {
                        var commandResult = command.Func(cmd, args);
                        // TODO: Split packets?
                        SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue,
                            commandResult));
                    }
                }

                if (!_connected)
                {
                    CloseConnection();
                    return;
                }

                _buffer = new byte[MaxAllowedPacketSize];
                _ns.BeginRead(_buffer, 0, MaxAllowedPacketSize, OnPacket, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}