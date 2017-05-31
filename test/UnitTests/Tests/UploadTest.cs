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
                var schmittAvatar = Image.FromWeb(new Uri("http://i.epvpimg.com/wwfRdab.jpg"));
                var gitAvatar = Image.FromFileSystem("../../Resources/identicon.png");

                if (!String.IsNullOrEmpty(TestEnvironment.Session.User.AvatarUrl))
                {
                    TestEnvironment.Session.Profile.RemoveAvatar(TestEnvironment.Session);
                    TestEnvironment.Session.User.Update(TestEnvironment.Session);

                    Assert.AreEqual(0, TestEnvironment.Session.User.AvatarUrl.Length, "The avatar of the logged-in user was not removed");
                }

                TestEnvironment.Session.Profile.SetAvatar(TestEnvironment.Session, schmittAvatar);
                TestEnvironment.Session.User.Update(TestEnvironment.Session);
                Assert.AreNotEqual(0, TestEnvironment.Session.User.AvatarUrl.Length, "The test avatar from the web was not uploaded and set");

                TestEnvironment.Session.Profile.SetAvatar(TestEnvironment.Session, gitAvatar);
                TestEnvironment.Session.User.Update(TestEnvironment.Session);
                Assert.AreNotEqual(0, TestEnvironment.Session.User.AvatarUrl, "The test avatar from the file system was not uploaded and set");
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
