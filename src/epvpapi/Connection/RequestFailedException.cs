using System;

namespace epvpapi.Connection
{
    [Serializable]
    /// <summary> Exception that is thrown if a request could not be sent or if the server did not respond as expected </summary>
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
