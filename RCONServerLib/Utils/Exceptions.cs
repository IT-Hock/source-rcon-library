using System;

namespace RCONServerLib.Utils
{
    public class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException()
        {
        }

        public NotAuthenticatedException(string message) : base(message)
        {
        }
    }

    public class EmptyPacketPayloadException : Exception
    {
        public EmptyPacketPayloadException()
        {
        }

        public EmptyPacketPayloadException(string message) : base(message)
        {
        }
    }

    public class PacketTooLongException : Exception
    {
        public PacketTooLongException()
        {
        }

        public PacketTooLongException(string message) : base(message)
        {
        }
    }

    public class NullTerminatorMissingException : Exception
    {
        public NullTerminatorMissingException()
        {
        }

        public NullTerminatorMissingException(string message) : base(message)
        {
        }
    }

    public class LengthMismatchException : Exception
    {
        public LengthMismatchException()
        {
        }

        public LengthMismatchException(string message) : base(message)
        {
        }
    }

    public class InvalidPacketTypeException : Exception
    {
        public InvalidPacketTypeException()
        {
        }

        public InvalidPacketTypeException(string message) : base(message)
        {
        }
    }
}