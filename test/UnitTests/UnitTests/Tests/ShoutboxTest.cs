using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using epvpapi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    public class ShoutboxTest
    {
        [TestMethod]
        public void TestHistory()
        {
            var globalHistoryShouts = Shoutbox.Global.History(TestEnvironment.Session, 1, 1);
            Assert.AreNotEqual(0, globalHistoryShouts.Count, "No shouts were pulled from the global channel history");

            // one page contains 15 shouts. If we're specifiying that we want to parse just one page, we shouldn't get more than 15 shouts
            if(globalHistoryShouts.Count > 15) 
                Assert.Fail("More than 15 shouts were returned");

            foreach (var shout in globalHistoryShouts)
            {
                Assert.AreNotEqual(0, shout.Content.Length);
                Assert.AreNotEqual(default(DateTime), shout.Date);
                Assert.AreNotEqual(0, shout.Sender.Name.Length);
                Assert.AreNotEqual(0, shout.Sender.ID);
            }
        }
    }
}
