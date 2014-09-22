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

        public struct Credentials
        {
            public uint ID { get; set; }
            public string Name { get; set; }
            public string MD5Hash { get; set; }
        }

        public static Credentials LoadTestCredentials()
        {
            var credentials = new Credentials();

            var credentialsFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/epvpapi/credentials.txt";
            using (var fs = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var fileContents = sr.ReadToEnd();
                    var splittedContents = fileContents.Split(':');
                    if (splittedContents.Count() == 3)
                    {
                        credentials.Name = splittedContents.First();
                        credentials.ID = Convert.ToUInt32(splittedContents[1]);
                        credentials.MD5Hash = splittedContents[2];
                    }
                }
            }

            return credentials;
        }
    }
}
