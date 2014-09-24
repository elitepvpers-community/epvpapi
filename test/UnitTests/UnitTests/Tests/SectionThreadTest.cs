using epvpapi;
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
            // http://www.elitepvpers.com/forum/main/619408-shoutbox-rules-regeln.html
            var testThread = new SectionThread(619408, Section.Main);
            var replies = testThread.Replies(TestEnvironment.Session);
            Assert.AreNotEqual(0, replies.Count, "No replies were pulled from the test thread");

            foreach(var reply in replies)
            {
                Assert.AreNotEqual(0, reply.ID, "The ID of a reply was not set");
                Assert.AreNotEqual(0, reply.Content.Length, "The content of a reply was not set");
                Assert.AreNotEqual(default(DateTime), reply.Date, "The date and time of a reply was not set");
                Assert.AreNotEqual(0, reply.Sender.ID, "The ID of a reply sender was not set");
                Assert.AreNotEqual(0, reply.Sender.Name.Length, "The name of a reply was not set");
                Assert.AreNotEqual(0, reply.Sender.Title.Length, "The user title of a reply sender was not set");
                Assert.AreNotEqual(default(DateTime), reply.Sender.JoinDate, "The join date of a reply sender was not set");
            }
        }
    }
}
