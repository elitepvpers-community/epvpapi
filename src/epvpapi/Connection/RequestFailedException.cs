using System;

namespace epvpapi.Connection
{
    /// <summary> Exception thrown if a request could not be executed or if the server did not respond as expected </summary>
    public class RequestFailedException : EpvpapiException
    {
        public RequestFailedException(string message)
            : base(message)
        { }

        public RequestFailedException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
