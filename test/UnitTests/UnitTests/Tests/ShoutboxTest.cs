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
    public class ShoutboxTest
    {
        [TestMethod]
        public void TestHistory()
        {
            try
            {

            var globalHistoryShouts = Shoutbox.Global.History(TestEnvironment.Session, 1, 1);
            Assert.AreNotEqual(0, globalHistoryShouts.Count, "No shouts were pulled from the global channel history");

            // one page contains 15 shouts. If we're specifiying that we want to parse just one page, we shouldn't get more than 15 shouts
                if (globalHistoryShouts.Count > 15)
                    Assert.Fail("More than 15 shouts were parsed");

            foreach (var shout in globalHistoryShouts)
            {
                    Assert.AreNotEqual(0, shout.Message.Length, "The message content of a shout was not set");
                    Assert.AreNotEqual(default(DateTime), shout.Date, "The date and time of a shout was not set");
                    Assert.AreNotEqual(0, shout.Sender.Name.Length, "The name of the shout sender was not set");
                    Assert.AreNotEqual(0, shout.Sender.ID, "The ID of the shout sender was not set");
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
        public void TestUpdate()
        {
            try
            {
                Shoutbox.Update(TestEnvironment.Session);

                if (Shoutbox.TopChatter.Count > 10)
                    Assert.Fail("More than 10 top chatters were parsed");

                foreach (var chatter in Shoutbox.TopChatter)
                    Assert.AreNotEqual(0, chatter.Name.Length, "The name of a top chatter was not set");
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
        public void TestShouts()
        {
            try
            {
                var shouts = Shoutbox.Global.Shouts(TestEnvironment.Session);
                Assert.AreNotEqual(0, shouts.Count, "No shouts were parsed");

                if (shouts.Count > 15)
                    Assert.Fail("More than 15 shouts were parsed");

                foreach (var shout in shouts)
                {
                    Assert.AreNotEqual(0, shout.Content.Length, "The message content of a shout was not set");
                    Assert.AreNotEqual(default(DateTime), shout.Date, "The date and time of a shout was not set");
                    Assert.AreNotEqual(0, shout.Sender.Name.Length, "The name of the shout sender was not set");
                    Assert.AreNotEqual(0, shout.Sender.ID, "The ID of the shout sender was not set");
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
