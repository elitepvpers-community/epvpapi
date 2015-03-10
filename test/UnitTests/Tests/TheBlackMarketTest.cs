using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using epvpapi;
using epvpapi.Connection;
using epvpapi.TBM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Tests
{
    [TestClass]
    public class TheBlackMarketTest
    {
        private void TestGetTreasures(Treasure.Query targetStatus)
        {
            try
            {
                var treasures = TestEnvironment.Session.Profile.GetTreasures(TestEnvironment.Session, targetStatus, 1, 1);
                if (treasures.Count == 0)
                    Assert.Fail("No treasures were retrieved");

                if (treasures.Count > 15)
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
        public void TestGetSoldListedTreasures()
        {
            TestGetTreasures(Treasure.Query.SoldListed);
        }

        [TestMethod]
        public void TestGetBoughtTreasures()
        {
            TestGetTreasures(Treasure.Query.Bought);
        }

        [TestMethod]
        public void TestGetTransactions()
        {
            try
            {
                TestEnvironment.Session.User.TBMProfile.SecretWord = TestEnvironment.TestCredentials.SecretWord;
                var transactions = TestEnvironment.Session.User.TBMProfile.GetTransactions(TestEnvironment.Session);
                Assert.AreNotEqual(0, transactions.Count, "No transactions were found");

                foreach (var transaction in transactions)
                {
                    Assert.AreNotEqual(0, transaction.EliteGold, "elite*gold amount of a transaction was not set");
                    Assert.AreNotEqual(default(DateTime), transaction.Time, "Date and time of a transaction was not set");
                    Assert.AreNotEqual(0, transaction.Sender.ID, "The ID of the transaction sender was not set");
                    Assert.AreNotEqual(0, transaction.Sender.Name.Length,
                        "The name of the transaction sender was not set");
                    Assert.AreNotEqual(0, transaction.Receiver.ID, "The ID of the transaction receiver was not set");
                    Assert.AreNotEqual(0, transaction.Receiver.Name.Length,
                        "The name of the transaction receiver was not set");
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
            catch (InvalidAuthenticationException exc)
            {
                AssertExtender.Exception("Secret word is invalid", exc);
            }
        }
    }
}
