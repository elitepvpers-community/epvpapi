using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class PrivateMessage : Post
    {
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
            : base(content, title)
        {
            Recipients = recipients;
        }

        /// <summary>
        /// Sends a <c>PrivateMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <remarks>
        /// The names of the recipients have to be given in order to send the message
        /// </remarks>
        public void Send(Session session)
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
    }
}
