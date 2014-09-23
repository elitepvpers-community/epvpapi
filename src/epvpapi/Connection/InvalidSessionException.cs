using System;

namespace epvpapi.Connection
{
    public class InvalidSessionException : EpvpapiException
    {
        public InvalidSessionException(string message)
            : base(message)
        { }
    }
}
