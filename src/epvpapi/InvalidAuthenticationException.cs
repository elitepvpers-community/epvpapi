using System;

namespace epvpapi
{
    public class InvalidAuthenticationException : Exception
    {
        public InvalidAuthenticationException(string message)
            : base(message)
        { }
    }
}
