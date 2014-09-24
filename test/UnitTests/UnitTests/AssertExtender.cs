using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using epvpapi;
using epvpapi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    public static class AssertExtender
    {
        public static void Exception(string msg, Exception exception)
        {
            Assert.Fail(msg + "\nException: " + exception.ToString());
        }    
    }
}
