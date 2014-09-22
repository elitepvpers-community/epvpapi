using System;
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
                Helper.AssertEmptyString(testSession.SecurityToken, "The session's security token was not detected");

                // parse only the first page
                var messages = testSession.ConnectedProfile.GetPrivateMessages(1, 1, PrivateMessage.Folder.Received);

                if (messages.Count <= 0)
                    Assert.Fail("Messages could not be parsed or no private messages do exist");

                // check if the properties of all messages were parsed correctly
                foreach (var message in messages)
                {
                    Assert.AreNotEqual(0, message.ID, "The ID of a private message was not set");
                    if(new DateTime() == message.Date)
                        Assert.Fail("The date and time of a private message was not set");
                    Helper.AssertEmptyString(message.Title, "The title of a private message was not set");
                    Assert.AreNotEqual(0, message.Sender.ID);
                    Helper.AssertEmptyString(message.Sender.Name, "The name of a sender of a private message was not set");

                    // update the private message and retrieve extra information
                    // since this function also updates the properties handled before, we'll check them again
                    message.Update(testSession);

                    Assert.AreNotEqual(0, message.ID, "The ID of a private message was not set after updating");
                    if (new DateTime() == message.Date)
                        Assert.Fail("The date and time of a private message was not set after updating");
                    Helper.AssertEmptyString(message.Title, "The title of a private message was not set after updating");
                    Assert.AreNotEqual(0, message.Sender.ID);
                    Helper.AssertEmptyString(message.Sender.Name, "The name of a sender of a private message was not set after updating");
                    Helper.AssertEmptyString(message.Content, "The content of a private message was not set after updating");
                }
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
