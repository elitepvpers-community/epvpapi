using System;

namespace epvpapi
{
    [Serializable]
    public class InsufficientAccessException : EpvpapiException
    {
        public InsufficientAccessException(string message)
            : base(message)
        { }
    }
}
