using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class InsufficientAccessException : Exception
    {
        public InsufficientAccessException(string message)
            : base(message)
        { }
    }
}
