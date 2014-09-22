using epvpapi;
using epvpapi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    public class SessionTest
    {
        [TestMethod]
        public void TestLogin()
        {
            try
            {
                var testCredentials = Helper.LoadTestCredentials();
                var session = new ProfileSession<PremiumUser>(new PremiumUser(testCredentials.Name, testCredentials.ID), testCredentials.MD5Hash);
                Assert.AreEqual(true, session.Valid, "The session is invalid");
                Assert.IsNotNull(session.SecurityToken, "The session's security token was not detected");
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
