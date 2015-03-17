using System;

namespace epvpapi
{
    [Serializable]
    public class EpvpapiException : Exception
    {
        public EpvpapiException(string msg) :
            base(msg)
        { }

        public EpvpapiException(string msg, Exception inner) :
            base(msg, inner)
        { }
    }
}
