using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using epvpapi.TBM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    public class TheBlackMarketTest
    {
        public void TestGetTreasures(Treasure.Query targetStatus)
        {
            var treasures = TestEnvironment.Session.ConnectedProfile.GetTreasures(targetStatus, 1, 1);
            if (treasures.Count == 0)
                Assert.Fail("No treasures were retrieved");

            if(treasures.Count > 15)
                Assert.Fail("Too much treasures were retrieved");

            foreach (var treasure in treasures)
            {
                Assert.AreNotEqual(0, treasure.ID, "The ID of a treasure was not set");
                Assert.AreNotEqual(0, treasure.Title.Length, "The title of a treasure was not set");
                Assert.AreNotEqual(0, treasure.Cost, "The cost of a treasure was not set");

                // The bought request needs to have the seller property set while the selled/listed query does not
                if (targetStatus == Treasure.Query.Bought)
                {
                    Assert.AreNotEqual(0, treasure.Seller.ID);
                    Assert.AreNotEqual(0, treasure.Seller.Name.Length);
                }
            }
        }

        [TestMethod]
        public void TestSoldListedTreasures()
        {
            TestGetTreasures(Treasure.Query.SoldListed);
        }

        [TestMethod]
        public void TestBoughtTreasures()
        {
            TestGetTreasures(Treasure.Query.Bought);
        }
    }
}
