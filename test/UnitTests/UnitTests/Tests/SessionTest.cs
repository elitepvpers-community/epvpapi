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
                Helper.LoadTestCredentials();
                Helper.TestSession = new ProfileSession<PremiumUser>(new PremiumUser(Helper.TestCredentials.Name, Helper.TestCredentials.ID), Helper.TestCredentials.MD5Hash);
                Assert.AreEqual(true, Helper.TestSession.Valid, "The session is invalid");
                Assert.IsNotNull(Helper.TestSession.SecurityToken, "The session's security token was not detected");
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
