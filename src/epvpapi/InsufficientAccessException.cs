using System;

namespace epvpapi
{
    public class InsufficientAccessException : EpvpapiException
    {
        public InsufficientAccessException(string message)
            : base(message)
        { }
    }
}
