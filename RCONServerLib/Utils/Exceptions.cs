using System;

namespace RCONServerLib.Utils
{
    public class RconServerException : Exception
    {
        protected RconServerException()
        {
        }

        protected RconServerException(string message) : base(message)
        {
        }
    }

    public class NotAuthenticatedException : RconServerException
    {
        public NotAuthenticatedException()
        {
        }

        public NotAuthenticatedException(string message) : base(message)
        {
        }
    }

    public class EmptyPacketPayloadException : RconServerException
    {
        public EmptyPacketPayloadException()
        {
        }

        public EmptyPacketPayloadException(string message) : base(message)
        {
        }
    }

    public class PacketTooLongException : RconServerException
    {
        public PacketTooLongException()
        {
        }

        public PacketTooLongException(string message) : base(message)
        {
        }
    }

    public class NullTerminatorMissingException : RconServerException
    {
        public NullTerminatorMissingException()
        {
        }

        public NullTerminatorMissingException(string message) : base(message)
        {
        }
    }

    public class LengthMismatchException : RconServerException
    {
        public LengthMismatchException()
        {
        }

        public LengthMismatchException(string message) : base(message)
        {
        }
    }

    public class InvalidPacketTypeException : RconServerException
    {
        public InvalidPacketTypeException()
        {
        }

        public InvalidPacketTypeException(string message) : base(message)
        {
        }
    }
}