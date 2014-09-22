using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using epvpapi;
using epvpapi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    public class UploadTest
    {
        [TestMethod]
        public void AvatarTest()
        {
            try
            {
                var testCredentials = Helper.LoadTestCredentials();
                var testSession = new ProfileSession<PremiumUser>(new PremiumUser(testCredentials.Name, testCredentials.ID), testCredentials.MD5Hash);
                Assert.AreEqual(true, testSession.Valid, "The session is invalid");
                Helper.AssertEmptyString(testSession.SecurityToken, "The session's security token was not detected");

                var schmittAvatar = Image.FromWeb(new Uri("http://i.epvpimg.com/GQp1h.jpg"));
                var gitAvatar = Image.FromFileSystem("../../Resources/identicon.png");

                if (!String.IsNullOrEmpty(testSession.User.AvatarURL))
                {
                    testSession.ConnectedProfile.RemoveAvatar();
                    testSession.User.Update(testSession);

                    Helper.AssertNonEmptyString(testSession.User.AvatarURL, "The avatar of the logged-in user was not removed");
                }

                testSession.ConnectedProfile.SetAvatar(schmittAvatar);
                testSession.User.Update(testSession);
                Helper.AssertEmptyString(testSession.User.AvatarURL, "The test avatar from the web was not uploaded and set");

                testSession.ConnectedProfile.SetAvatar(gitAvatar);
                testSession.User.Update(testSession);
                Helper.AssertEmptyString(testSession.User.AvatarURL, "The test avatar from the file system was not uploaded and set");
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
