using System;

namespace epvpapi
{
    public class InvalidAuthenticationException : EpvpapiException
    {
        public InvalidAuthenticationException(string message)
            : base(message)
        { }
    }
}
