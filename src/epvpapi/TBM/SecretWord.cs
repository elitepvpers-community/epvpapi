using epvpapi.Connection;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi.TBM
{
    /// <summary>
    /// Represents the Secret word used for the official TBM API in order to query Transactions
    /// </summary>
    public class SecretWord
    {
        public string Value { get; set; }

        public SecretWord(string value = null)
        {
            Value = value;
        }

        /// <summary>
        /// Updates and gets the current Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <returns> Current Secret word as string </returns>
        public static SecretWord Get<TUser>(Session<TUser> session) where TUser : User
        {
            session.ThrowIfInvalid();

            var res = session.Get("http://www.elitepvpers.com/theblackmarket/api/secretword/");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            return new SecretWord(doc.DocumentNode.Descendants().GetElementsByNameXHtml("secretword").FirstOrDefault().Attributes["value"].Value);
        }

        /// <summary>
        /// Sets the Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Set<TUser>(Session<TUser> session) where TUser : User
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/theblackmarket/api/secretword/",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("secretword", Value)
                        });
        }
    }
}
