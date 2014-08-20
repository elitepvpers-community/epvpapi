using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class ParsingFailedException : Exception
    {
        public ParsingFailedException(string msg, Exception inner)
            : base(msg, inner)
        { }
    }
}
