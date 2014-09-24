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
    public class SectionTest
    {
        [TestMethod]
        public void TestUpdate()
        {
            try
            {
                Section.Main.Update(TestEnvironment.Session);

                if (Section.Main.Announcements.Count < 3) // every section needs to supply at least 3 announcements
                    Assert.Fail("The announcements were not fully detected. Announcement count: {0}",
                                Section.Main.Announcements.Count);

                foreach (var announcement in Section.Main.Announcements)
                {
                    Assert.AreNotEqual(0, announcement.Title.Length, "The title of an announcement was not set");
                    Assert.AreNotEqual(default(DateTime), announcement.Begins,
                                        "The begin date of the announcement was not set");
                    Assert.AreNotEqual(0, announcement.Sender.ID, "The ID of the announcement creator was not set");
                    Assert.AreNotEqual(0, announcement.Sender.Name, "The name of the announcement creator was not set");
                    Assert.AreNotEqual(0, announcement.Sender.Title,
                                        "The user title of the announcement creator was not set");
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

        [TestMethod]
        public void TestThreads()
        {
            try
            {
                var threads = Section.Main.Threads(TestEnvironment.Session, 1, 1);
                Assert.AreNotEqual(0, threads.Count, "No threads could be retrieved");

                foreach (var thread in threads)
                {
                    Assert.AreNotEqual(0, thread.ID, "The ID of a thread was not set");
                    Assert.AreNotEqual(0, thread.InitialPost.Title.Length, "The thread title was not set");
                    Assert.AreNotEqual(0, thread.Creator.ID, "The ID of the thread creator was not set");
                    Assert.AreNotEqual(0, thread.Creator.Name.Length, "The name of the thread creator was not set");
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
