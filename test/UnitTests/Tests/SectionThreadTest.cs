using epvpapi;
using epvpapi.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Tests
{
    [TestClass]
    public class SectionThreadTest
    {
        [TestMethod]
        public void TestReplies()
        {
            // http://www.elitepvpers.com/forum/main/1329965-de-en-infractions-warnings-things-you-should-know.html
            var testThread = new SectionThread(1329965, Section.Main);

            try
            {
                var replies = testThread.Replies(TestEnvironment.Session);
                Assert.AreNotEqual(0, replies.Count, "No replies were pulled from the test thread");
                Assert.AreNotEqual(0, testThread.InitialPost.ID, "The initial post of the test thread was not set");

                foreach (var reply in replies)
                {
                    Assert.AreNotEqual(0, reply.ID, "The ID of a reply was not set");
                    Assert.AreNotEqual(0, reply.Content.ToString().Length, "The content of a reply was not set");
                    Assert.AreNotEqual(default(DateTime), reply.Date, "The date and time of a reply was not set");
                    Assert.AreNotEqual(0, reply.Sender.ID, "The ID of a reply sender was not set");
                    Assert.AreNotEqual(0, reply.Sender.Name.Length, "The name of a reply sender was not set");
                    Assert.AreNotEqual(0, reply.Sender.Title.Length, "The user title of a reply sender was not set");
                    Assert.AreNotEqual(default(DateTime), reply.Sender.JoinDate,
                        "The join date of a reply sender was not set");
                }
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
