using System;

namespace epvpapi
{
    public class ParsingFailedException : Exception
    {
        public ParsingFailedException(string msg):
            base(msg)
        { }

        public ParsingFailedException(string msg, Exception inner)
            : base(msg, inner)
        { }
    }
}
