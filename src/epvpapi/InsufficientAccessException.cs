using System;

namespace epvpapi
{
    public class InsufficientAccessException : Exception
    {
        public InsufficientAccessException(string message)
            : base(message)
        { }
    }
}
