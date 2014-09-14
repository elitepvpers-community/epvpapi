using epvpapi.Connection;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi.TBM
{
    /// <summary>
    /// Represents the Secret word used for the official TBM API in order to query Transactions
    /// </summary>
    public static class SecretWord
    {
        /// <summary>
        /// Updates and gets the current Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <returns> Current Secret word as string </returns>
        public static string Get(Session session)
        {
            session.ThrowIfInvalid();

            Response res = session.Get("http://www.elitepvpers.com/theblackmarket/api/secretword/");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            return doc.DocumentNode.GetDescendentElementsByNameXHTML("secretword").FirstOrDefault().Attributes["value"].Value;
        }

        /// <summary>
        /// Sets the Secret word
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        /// <param name="secretWord"> New value to set as Secret word </param>
        public static void Set(Session session, string secretWord)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/theblackmarket/api/secretword/",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("secretword", secretWord)
                        });
        }
    }
}
