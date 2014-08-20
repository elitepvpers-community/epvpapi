using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi.Connection
{
    public class InvalidSessionException : Exception
    {
        public InvalidSessionException(string message)
            : base(message)
        { }
    }
}
