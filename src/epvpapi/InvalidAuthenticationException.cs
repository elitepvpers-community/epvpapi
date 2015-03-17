using System;

namespace epvpapi
{
    [Serializable]
    public class InvalidAuthenticationException : EpvpapiException
    {
        public InvalidAuthenticationException(string message)
            : base(message)
        { }
    }
}
