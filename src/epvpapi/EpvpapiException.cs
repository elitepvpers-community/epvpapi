using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class EpvpapiException : Exception
    {
        public EpvpapiException(string msg) :
            base(msg)
        { }

        public EpvpapiException(string msg, Exception inner):
            base(msg, inner)
        { }
    }
}
