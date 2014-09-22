using epvpapi;
using epvpapi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class PrivateMessagesTest
    {
        [TestMethod]
        public void TestGetPrivateMessages()
        {
            try
            {
                var testCredentials = Helper.LoadTestCredentials();
                var testSession = new ProfileSession<PremiumUser>(new PremiumUser(testCredentials.Name, testCredentials.ID), testCredentials.MD5Hash);
                Assert.AreEqual(true, testSession.Valid, "The session is invalid");
                Assert.IsNotNull(testSession.SecurityToken, "The session's security token was not detected");

                var messages = testSession.ConnectedProfile.GetPrivateMessages(PrivateMessage.Folder.Received);
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
            catch (InvalidSessionException exc)
            {
                Helper.AssertException("Session is invalid", exc);
            }
        }
    }
}
