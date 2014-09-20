using System;
using System.Collections.Generic;
using System.Linq;
using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;

namespace epvpapi
{
    public class PrivateMessage : Post, IReportable, IDefaultUpdatable, IUniqueWebObject
    {
        /// <summary>
        ///     Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public new enum Settings
        {
            /// <summary>
            ///     If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,

            /// <summary>
            ///     If set, the signature of the logged in user will be displayed beneath the message
            /// </summary>
            ShowSignature = 2,

            /// <summary>
            ///     If set, every time a message will be send, a copy will be saved and stored somewhere
            /// </summary>
            SaveCopy = 3
        }

        public PrivateMessage(uint id)
            : this(id, null, new List<User>())
        {
        }

        public PrivateMessage(User recipient, string content, string title = null)
            : this(0, content, new List<User> {recipient}, title)
        {
        }

        public PrivateMessage(List<User> recipients, string content, string title = null)
            : this(0, content, recipients, title)
        {
        }

        private PrivateMessage(uint id, string content, List<User> recipients, string title = null)
            : base(id, content, title)
        {
            Recipients = recipients;
        }

        /// <summary>
        ///     Recipients of the message
        /// </summary>
        public List<User> Recipients { private get; set; }

        /// <summary>
        ///     If true, the message is marked as new message that wasn't read yet
        /// </summary>
        public bool Unread { get; set; }

        /// <summary>
        ///     Retrieves information about the messages such as title, content and sender
        /// </summary>
        /// <param name="session"> Session used for sending the request </param>
        public void Update(Session session)
        {
            session.ThrowIfInvalid();
            if (Id == 0) throw new ArgumentException("ID must not be emtpy");

            Response res = session.Get("http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + Id);
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            new SenderParser(Sender).Execute(doc.GetElementbyId("post"));
            new ContentParser(this).Execute(doc.GetElementbyId("td_post_"));
        }

        public void Report(Session session, string reason)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/private.php?do=sendemail",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("reason", reason),
                    new KeyValuePair<string, string>("pmid", Id.ToString()),
                    new KeyValuePair<string, string>("do", "sendemail"),
                    new KeyValuePair<string, string>("url",
                        "http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + Id)
                });
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + Id;
        }

        /// <summary>
        ///     Sends a <c>PrivateMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        ///     The names of the recipients have to be given in order to send the message.
        ///     Messages with a blank title will not be send. Therefore, '-' will be used as title if nothing was specified.
        ///     Certain requirements must be fulfilled in order to send messages automatically without entering a captcha:
        ///     - More than 20 posts OR the <c>User.Rank.Premium</c> rank OR the <c>User.Rank.EliteGoldTrader</c> rank
        /// </remarks>
        public void Send<T>(ProfileSession<T> session, Settings settings = Settings.ParseUrl | Settings.ShowSignature)
            where T : User
        {
            session.ThrowIfInvalid();
            if (session.User.Posts <= 20 && !session.User.HasRank(User.Rank.Premium) &&
                !session.User.HasRank(User.Rank.EliteGoldTrader))
                throw new InsufficientAccessException(
                    "More than 20 posts or the premium / elite*gold trader badge is required for sending private messages without captchas");

            string recipients = "";
            foreach (User recipient in Recipients)
            {
                recipients += recipient.Name;
                if (recipient != Recipients.Last())
                    recipients += ";";
            }

            session.Post("http://www.elitepvpers.com/forum/private.php?do=insertpm&pmid=",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("recipients", recipients),
                    new KeyValuePair<string, string>("bccrecipients", String.Empty),
                    new KeyValuePair<string, string>("title", Title ?? "-"),
                    new KeyValuePair<string, string>("message", Content),
                    new KeyValuePair<string, string>("wysiwyg", "0"),
                    new KeyValuePair<string, string>("iconid", "0"),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "insertpm"),
                    new KeyValuePair<string, string>("pmid", String.Empty),
                    new KeyValuePair<string, string>("forward", String.Empty),
                    new KeyValuePair<string, string>("sbutton", "submit"),
                    new KeyValuePair<string, string>("savecopy", (settings & Settings.SaveCopy).ToString()),
                    new KeyValuePair<string, string>("signature", (settings & Settings.ShowSignature).ToString()),
                    new KeyValuePair<string, string>("parseurl", (settings & Settings.ParseUrl).ToString())
                });
        }

        private class ContentParser : TargetableParser<PrivateMessage>, INodeParser
        {
            public ContentParser(PrivateMessage target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                HtmlNode titleNode = coreNode.SelectSingleNode("div[1]/strong[1]");
                Target.Title = (titleNode != null) ? titleNode.InnerText : "";

                // The actual message content is stored within several nodes. There may be different tags (such as <a> for links, linebreaks...)
                // This is why just all descendent text nodes are retrieved.
                var contentNodes = new List<HtmlNode>(coreNode.SelectSingleNode("div[2]").Descendants()
                    .Where(node => node.Name == "#text" && node.InnerText.Strip() != ""));
                contentNodes.ForEach(node => Target.Content += node.InnerText);
            }
        }

        public class Folder
        {
            public enum Storage
            {
                Sent,
                Received
            }

            private Folder(int id, string name = null, Storage storageType = Storage.Received)
            {
                ID = id;
                Name = name;
                StorageType = storageType;
            }

            public int ID { get; private set; }
            private string Name { get; set; }
            public Storage StorageType { get; private set; }

            public static Folder Sent
            {
                get { return new Folder(-1, "Sent", Storage.Sent); }
            }

            public static Folder Received
            {
                get { return new Folder(0, "Received"); }
            }
        }

        private class SenderParser : TargetableParser<User>, INodeParser
        {
            public SenderParser(User target) : base(target)
            {
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;
                coreNode = coreNode.SelectSingleNode("tr[2]/td[1]/div[1]/a[1]");
                if (coreNode == null) return;

                HtmlNode userNameNode = coreNode.SelectSingleNode("span[1]");

                Target.Name = (userNameNode != null) ? userNameNode.InnerText : "";
                Target.Id = (coreNode.Attributes.Contains("href")) ? User.FromUrl(coreNode.Attributes["href"].Value) : 0;
            }
        }
    }
}