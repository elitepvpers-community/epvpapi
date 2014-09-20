using System.Collections.Generic;
using System.Linq;
using epvpapi.Connection;
using HtmlAgilityPack;

namespace epvpapi.TBM
{
    /// <summary>
    ///     Represents the Secret word used for the official TBM API in order to query Transactions
    /// </summary>
    public class SecretWord
    {
        public SecretWord(string value = null)
        {
            Value = value;
        }

        public string Value { get; private set; }

        /// <summary>
        ///     Updates and gets the current Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <returns> Current Secret word as string </returns>
        public static SecretWord Get(Session session)
        {
            session.ThrowIfInvalid();

            Response res = session.Get("http://www.elitepvpers.com/theblackmarket/api/secretword/");
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            return
                new SecretWord(
                    doc.DocumentNode.GetDescendentElementsByNameXhtml("secretword").FirstOrDefault().Attributes["value"]
                        .Value);
        }

        /// <summary>
        ///     Sets the Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Set(Session session)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/theblackmarket/api/secretword/",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("secretword", Value)
                });
        }
    }
}