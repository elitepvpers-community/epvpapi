using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using epvpapi;
using epvpapi.Connection;
using epvpapi.Generation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    class Program
    {
        [TestMethod]
        public void GetPrivateMessages()
        {
            try
            {
                var testCredentials = Helper.LoadTestCredentials();
                var session = new ProfileSession<User>(new User(testCredentials.Name, testCredentials.ID), testCredentials.MD5Hash);
                var profile = session.ConnectedProfile;
                var messages = profile.GetPrivateMessages(PrivateMessage.Folder.Received);
                if (messages.Count <= 0)
                    Assert.Fail("Messages could not be parsed or don't exist");
            }
            catch (InvalidAuthenticationException exc)
            {
                Helper.AssertException("The user was not logged in", exc);
            }
            catch (RequestFailedException exc)
            {
                Helper.AssertException("A HTTP request failed", exc);
            }
        }
    }
}
