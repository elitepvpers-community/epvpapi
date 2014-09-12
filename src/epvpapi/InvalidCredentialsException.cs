using System;

namespace epvpapi
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message)
            : base(message)
        { }
    }
}
