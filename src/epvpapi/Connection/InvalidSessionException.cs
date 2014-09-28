using System;

namespace epvpapi.Connection
{
    /// <summary>
    /// Exception that is thrown if the session being used is invalid, i.e. if no user is linked to the session.
    /// </summary>
    public class InvalidSessionException : EpvpapiException
    {
        public InvalidSessionException(string message)
            : base(message)
        { }
    }
}
