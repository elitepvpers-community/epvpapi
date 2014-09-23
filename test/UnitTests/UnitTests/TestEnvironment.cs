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
    public static class TestEnvironment
    {
        public struct Credentials
        {
            public uint ID { get; set; }
            public string Name { get; set; }
            public string MD5Hash { get; set; }
            public string SecretWord { get; set; }
        }

        private static Credentials LoadTestCredentials()
        {
            var credentials = new Credentials();

            var credentialsFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/epvpapi/credentials.txt";
            using (var fs = new FileStream(credentialsFile, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var fileContents = sr.ReadToEnd();
                    var splittedContents = fileContents.Split(':');
                    if (splittedContents.Count() == 4)
                    {
                        credentials.Name = splittedContents.First();
                        credentials.ID = Convert.ToUInt32(splittedContents[1]);
                        credentials.MD5Hash = splittedContents[2];
                        credentials.SecretWord = splittedContents[3];
                    }
                }
            }

            return credentials;
        }

        private static ProfileSession<PremiumUser> _Session;
        public static ProfileSession<PremiumUser> Session
        {
            get
            {
                if (_Session == null)
                    Prepare();

                if(!_Session.Valid)
                    Prepare();

                return _Session;
            }
        }

        public static Credentials TestCredentials { get; set; }

        private static void Prepare()
        {
            try
            {
                TestCredentials = LoadTestCredentials();
                _Session = new ProfileSession<PremiumUser>(new PremiumUser(TestCredentials.Name, TestCredentials.ID), TestCredentials.MD5Hash);
                Assert.AreEqual(true, _Session.Valid, "The session is invalid");
                AssertExtender.EmptyString(_Session.SecurityToken, "The session's security token was not detected");
            }
            catch (InvalidAuthenticationException exc)
            {
                AssertExtender.Exception("The user was not logged in", exc);
            }
            catch (RequestFailedException exc)
            {
                AssertExtender.Exception("A HTTP request failed", exc);
            }
            catch (InvalidSessionException exc)
            {
                AssertExtender.Exception("Session is invalid", exc);
            }
        }
    }
}
