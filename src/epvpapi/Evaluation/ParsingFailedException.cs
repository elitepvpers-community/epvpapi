﻿using System;

namespace epvpapi.Evaluation
{
    [Serializable]
    public class ParsingFailedException : EpvpapiException
    {
        public ParsingFailedException(string msg):
            base(msg)
        { }

        public ParsingFailedException(string msg, Exception inner)
            : base(msg, inner)
        { }
    }
}
