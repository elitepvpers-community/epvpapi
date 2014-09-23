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
    public static class Helper
    {
        public static void AssertException(string msg, Exception exception)
        {
            Assert.Fail(msg + "\nException: " + exception.ToString());
        }

        public static void AssertEmptyString(string str, string msg)
        {
            if (String.IsNullOrEmpty(str))
                Assert.Fail(msg);
        }

        public static void AssertNonEmptyString(string str, string msg)
        {
            if (!String.IsNullOrEmpty(str))
                Assert.Fail(msg);
        }


       
    }
}
