using epvpapi.Connection;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class PrivateMessage : Post, IReportable
    {
        /// <summary>
        /// User that sent the message
        /// </summary>
        User Sender { get; set; }

        /// <summary>
        /// Recipients of the message
        /// </summary>
        List<User> Recipients { get; set; }

        public PrivateMessage(uint id, string content = null)
            : this(id, content, new List<User>())
        { }

        public PrivateMessage(User recipient, string content, string title = null)
            : this(new List<User> { recipient }, content, title)
        { }

        public PrivateMessage(List<User> recipients, string content, string title = null)
            : this(0, content, recipients, title)
        {  }

        public PrivateMessage(uint id, string content, List<User> recipients, string title = null)
            : base(id, content, title)
        {
            Recipients = recipients;
            Sender = new User();
        }

        /// <summary>
        /// Sends a <c>PrivateMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        /// The names of the recipients have to be given in order to send the message
        /// </remarks>
        public void Send<T>(UserSession<T> session) where T : User
        {
            session.ThrowIfInvalid();

            string recipients = "";
            foreach(var recipient in Recipients)
            {
                recipients += recipient.Name;
                if (recipient != Recipients.Last())
                    recipients += ";";
            }

            session.Post("http://www.elitepvpers.com/forum/private.php?do=insertpm&pmid=",
                         new List<KeyValuePair<string, string>>()
                         {
                             new KeyValuePair<string, string>("recipients", recipients),
                             new KeyValuePair<string, string>("bccrecipients", String.Empty),
                             new KeyValuePair<string, string>("title", Title),
                             new KeyValuePair<string, string>("message", Content),
                             new KeyValuePair<string, string>("wysiwyg", "0"),
                             new KeyValuePair<string, string>("iconid", "0"),
                             new KeyValuePair<string, string>("s", String.Empty),
                             new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                             new KeyValuePair<string, string>("do", "insertpm"),
                             new KeyValuePair<string, string>("pmid", String.Empty),
                             new KeyValuePair<string, string>("forward", String.Empty),
                             new KeyValuePair<string, string>("sbutton", "submit"),
                             new KeyValuePair<string, string>("savecopy", (Settings & Options.SaveCopy).ToString()),
                             new KeyValuePair<string, string>("signature", (Settings & Options.ShowSignature).ToString()),
                             new KeyValuePair<string, string>("parseurl", (Settings & Options.ParseURL).ToString())
                         });
    
        }

        /// <summary>
        /// Retrieves information about the messages such as title, content and sender
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (ID == 0) throw new System.ArgumentException("ID must not be emtpy");

            Response res = session.Get("http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID.ToString());
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            HtmlNode messageRootNode = doc.GetElementbyId("post");
            if (messageRootNode == null) throw new ParsingFailedException("Private message could not be parsed, root node wasn't found or is invalid");

            HtmlNode userNode = messageRootNode.SelectSingleNode("tr[2]/td[1]/div[1]/a[1]");
            string userName = (userNode.SelectSingleNode("span[1]") != null) ? userNode.SelectSingleNode("span[1]").InnerText : "";
            Sender = (userNode != null) ? new User(userName, User.FromURL(userNode.Attributes["href"].Value)) : new User(userName);

            HtmlNode messageNode = doc.GetElementbyId("td_post_");
            if(messageNode != null)
            {
                HtmlNode titleNode = messageNode.SelectSingleNode("div[1]/strong[1]");
                Title = (titleNode != null) ? titleNode.InnerText : "";

                // The actual message content is stored within several nodes. There may be different tags (such as <a> for links, linebreaks...)
                // This is why just all descendent text nodes are retrieved.
                List<HtmlNode> contentNodes = new List<HtmlNode>(messageNode.SelectSingleNode("div[2]").Descendants()
                                                                .Where(node => node.Name == "#text" && node.InnerText.Strip() != ""));
                contentNodes.ForEach(node => Content += node.InnerText);
            }
        }

        public void Report(Session session, string reason)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/private.php?do=sendemail",
                         new List<KeyValuePair<string, string>>()
                         {
                             new KeyValuePair<string, string>("s", String.Empty),
                             new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                             new KeyValuePair<string, string>("reason", reason),
                             new KeyValuePair<string, string>("pmid", ID.ToString()),
                             new KeyValuePair<string, string>("do", "sendemail"),
                             new KeyValuePair<string, string>("url", "http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID.ToString())
                         });
        }
    }
}
