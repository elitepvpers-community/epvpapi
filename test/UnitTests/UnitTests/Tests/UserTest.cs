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
    public class UserTest
    {
        [TestMethod]
        public void TestUserSearch()
        {
            try
            {
                var foundUsers = User.Search(TestEnvironment.Session, TestEnvironment.Session.User.Name);
                Assert.AreNotEqual(0, foundUsers.Count, "No users were found for search term {0}", TestEnvironment.Session.User.Name);

                if (foundUsers.Count != 0)
                    Assert.AreNotEqual(0, foundUsers.First().ID, "ID of the searched user was not set");
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
        public void TestUserUpdate()
        {
            try
            {
                // copy the 2 required properties of the logged-in user to a new object to see if the update function works properly
                var testUser = new User(TestEnvironment.Session.User.Name, TestEnvironment.Session.User.ID);
                testUser.Update(TestEnvironment.Session);

                // Test the properties which always have to be set
                Assert.AreNotEqual(0, testUser.Title.Length, "Title of the user was not set");
                Assert.AreNotEqual(0, testUser.Namecolor.Length, "Namecolor of the user was not set");
                Assert.AreNotEqual(0, testUser.TBMProfile.ID, "ID of the user's TBM profile was not set");

                if (testUser.JoinDate == new DateTime() && testUser.CurrentStatus != User.Status.Invisible)
                    Assert.Fail("The join date of the user was not retrieved although the status was not detected invisible");
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
