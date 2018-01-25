using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using RCONServerLib.Utils;

namespace RCONServerLib
{
    /// <summary>
    ///     Client connected to the remote console
    /// </summary>
    internal class RemoteConClient
    {
        /// <summary>
        ///     The maximum packet size to receive
        /// </summary>
        private const int MaxAllowedPacketSize = 4096;

        /// <summary>
        ///     Used to determine if we're in unit test mode (Means no actual connection)
        /// </summary>
        private readonly bool _isUnitTest;

        /// <summary>
        ///     Underlaying NetworkStream
        /// </summary>
        private readonly NetworkStream _ns;

        /// <summary>
        ///     Reference to RemoteConServer for getting settings
        /// </summary>
        private readonly RemoteConServer _remoteConServer;

        /// <summary>
        ///     The TCP Client
        /// </summary>
        private readonly TcpClient _tcp;

        /// <summary>
        ///     How many failed login attempts the client has
        /// </summary>
        private int _authTries;

        /// <summary>
        ///     A buffer containing the packet
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        ///     Wether or not the client is connected
        /// </summary>
        private bool _connected;

        /// <summary>
        ///     Has the client been successfully authenticated
        /// </summary>
        internal bool Authenticated;

        public RemoteConClient(TcpClient tcp, RemoteConServer remoteConServer)
        {
            _tcp = tcp;
            _remoteConServer = remoteConServer;

            _ns = tcp.GetStream();
            _connected = true;
            Authenticated = false;

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
        ///     UnitTest only constructor
        /// </summary>
        internal RemoteConClient(RemoteConServer remoteConServer)
        {
            _remoteConServer = remoteConServer;
            _isUnitTest = true;
        }

        /// <summary>
        ///     Closes the TCP connection
        /// </summary>
        private void CloseConnection()
        {
            if (_isUnitTest)
                return;

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
            if (_isUnitTest)
                return;

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

                ParsePacket(_buffer);

                if (!_tcp.Connected)
                    return;

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

        /// <summary>
        ///     Parses raw bytes to RemoteConPacket
        /// </summary>
        /// <param name="rawPacket"></param>
        internal void ParsePacket(byte[] rawPacket)
        {
            try
            {
                var packet = new RemoteConPacket(rawPacket);

                // Do not allow any other packets than auth to be sent when client is not authenticated
                if (!Authenticated)
                {
                    if (packet.Type != RemoteConPacket.PacketType.Auth)
                    {
                        if (_isUnitTest)
                            throw new NotAuthenticatedException();
                        CloseConnection();
                    }

                    _authTries++;

                    if (packet.Payload == _remoteConServer.Password)
                    {
                        Authenticated = true;

                        if (!_remoteConServer.SendAuthImmediately)
                            SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue, ""));

                        SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ExecCommand, ""));
                        return;
                    }

                    if (_authTries >= _remoteConServer.MaxPasswordTries)
                    {
                        CloseConnection();
                        return;
                    }

                    if (!_remoteConServer.SendAuthImmediately)
                        SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue, ""));

                    SendPacket(new RemoteConPacket(-1, RemoteConPacket.PacketType.ExecCommand, ""));

                    return;
                }

                // Invalid packet type.
                if (packet.Type != RemoteConPacket.PacketType.ExecCommand)
                {
                    if (_isUnitTest)
                        throw new InvalidPacketTypeException();

                    if (_remoteConServer.InvalidPacketKick)
                        CloseConnection();
                    return;
                }

                if (packet.Payload == "")
                {
                    if (_isUnitTest)
                        throw new EmptyPacketPayloadException();

                    if (_remoteConServer.EmptyPayloadKick)
                        CloseConnection();
                    return;
                }

                var args = ArgumentParser.ParseLine(packet.Payload);
                var cmd = args[0];
                args.RemoveAt(0);

                if (_remoteConServer.UseCustomCommandHandler)
                {
                    var result = _remoteConServer.ExecuteCustomCommandHandler(cmd, args);
                    SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue,
                        result));
                    return;
                }

                var command = _remoteConServer.CommandManager.GetCommand(cmd);
                if (command == null)
                {
                    SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue,
                        "Invalid command \"" + packet.Payload + "\""));
                }
                else
                {
                    var commandResult = command.Handler(cmd, args);
                    // TODO: Split packets?
                    SendPacket(new RemoteConPacket(packet.Id, RemoteConPacket.PacketType.ResponseValue,
                        commandResult));
                }
            }
            catch (NotAuthenticatedException e)
            {
                throw;
            }
            catch (InvalidPacketTypeException e)
            {
                throw;
            }
            catch (EmptyPacketPayloadException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine(string.Format("Client {0} caused an exception: {1} and was killed.",
                    ((IPEndPoint) _tcp.Client.RemoteEndPoint).Address, e.Message));
                CloseConnection();
            }
        }
    }
}