using System;

namespace epvpapi.Connection
{
    public class InvalidSessionException : Exception
    {
        public InvalidSessionException(string message)
            : base(message)
        { }
    }
}
