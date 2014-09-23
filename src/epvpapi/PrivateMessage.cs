using epvpapi.Connection;
using epvpapi.Evaluation;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi
{
    public class PrivateMessage : Post, IReportable, IUpdatable, IUniqueWebObject
    {
        public class Folder
        {
            public enum Storage
            {
                Sent,
                Received
            }

            public int ID { get; set; }
            public string Name { get; set; }
            public Storage StorageType { get; set; }

            public static Folder Sent
            {
                get { return new Folder(-1, "Sent", Storage.Sent); }
            }

            public static Folder Received
            {
                get { return new Folder(0, "Received", Storage.Received); }
            }

            public Folder(int id, string name = null, Storage storageType = Storage.Received)
            {
                ID = id;
                Name = name;
                StorageType = storageType;
            }
        }

        /// <summary>
        /// Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public new enum Settings
        {
            /// <summary>
            /// If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseURL = 1,

            /// <summary>
            /// If set, the signature of the logged in user will be displayed beneath the message
            /// </summary>
            ShowSignature = 2,

            /// <summary>
            /// If set, every time a message will be send, a copy will be saved and stored somewhere
            /// </summary>
            SaveCopy = 3
        }

        /// <summary>
        /// Recipients of the message
        /// </summary>
        public List<User> Recipients { get; set; }

        /// <summary>
        /// If true, the message is marked as new message that wasn't read yet
        /// </summary>
        public bool Unread { get; set; }

        public PrivateMessage(uint id)
            : this(id, new List<VBContent>(), new List<User>())
        { }

        public PrivateMessage(User recipient, List<VBContent> contents, string title = null)
            : this(0, contents, new List<User>() { recipient }, title)
        {  }

        public PrivateMessage(List<User> recipients, List<VBContent> contents, string title = null)
            : this(0, contents, recipients, title)
        { }

        public PrivateMessage(uint id, List<VBContent> contents, List<User> recipients, string title = null)
            : base(id, contents, title)
        {
            Recipients = recipients;
        }

        /// <summary>
        /// Sends a <c>PrivateMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        /// The names of the recipients have to be given in order to send the message.
        /// Messages with a blank title will not be send. Therefore, '-' will be used as title if nothing was specified.
        /// Certain requirements must be fulfilled in order to send messages automatically without entering a captcha:
        /// - More than 20 posts OR the <c>User.Rank.Premium</c> rank OR the <c>User.Rank.EliteGoldTrader</c> rank
        /// </remarks>
        public void Send<T>(ProfileSession<T> session, Settings settings = Settings.ParseURL | Settings.ShowSignature) where T : User
        {
            session.ThrowIfInvalid();
            if (session.User.Posts <= 20 && !session.User.HasRank(User.Rank.Premium) && !session.User.HasRank(User.Rank.EliteGoldTrader))
                throw new InsufficientAccessException("More than 20 posts or the premium / elite*gold trader badge is required for sending private messages without captchas");

            var recipients = "";
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
                             new KeyValuePair<string, string>("title", Title ?? "-"),
                             new KeyValuePair<string, string>("message", Contents.ToString()),
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
                             new KeyValuePair<string, string>("parseurl", (settings & Settings.ParseURL).ToString())
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

            var res = session.Get("http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID.ToString());
            var doc = new HtmlDocument();
            doc.LoadHtml(res.ToString());

            new PrivateMessageParser.SenderParser(Sender).Execute(doc.GetElementbyId("post"));
            new PrivateMessageParser.ContentParser(this).Execute(doc.GetElementbyId("td_post_"));
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

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=" + ID;
        }  
    }
}
