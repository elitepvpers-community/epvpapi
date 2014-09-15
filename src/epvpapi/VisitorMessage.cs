using epvpapi.Connection;
using System;
using System.Collections.Generic;

namespace epvpapi
{
    /// <summary>
    /// Represents a message on the profile of an <c>User</c>
    /// </summary>
    public class VisitorMessage : Message, IUniqueWebObject
    {
        /// <summary>
        /// Recipient of the message
        /// </summary>
        public User Recipient { get; set; }

        public VisitorMessage(uint id)
            : base(id)
        {
            Recipient = new User();
        }
        
        public VisitorMessage(User recipient, string content)
            : base(content)
        {
            Recipient = recipient;
        }

        public VisitorMessage()
            : this(new User(), null)
        { }


        /// <summary>
        /// Sends a <c>VisitorMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        /// The ID of the recipient has to be given in order to send the message
        /// </remarks>
        public void Send(Session session, Message.Settings settings = Message.Settings.ParseURL)
        {
            if (Recipient.ID == 0) throw new ArgumentException("Recipient ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/visitormessage.php?do=message",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("ajax", "1"),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("styleid", "0"),
                            new KeyValuePair<string, string>("fromquickcomment", "1"),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("do", "message"),
                            new KeyValuePair<string, string>("u", Recipient.ID.ToString()),
                            new KeyValuePair<string, string>("u2", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", String.Empty),
                            new KeyValuePair<string, string>("parseurl", (settings & Settings.ParseURL).ToString()),
                            new KeyValuePair<string, string>("lastcomment", "1381528657"),
                            new KeyValuePair<string, string>("allow_ajax_qc", "1"),
                            new KeyValuePair<string, string>("fromconverse", String.Empty),
                            new KeyValuePair<string, string>("message", Content),
                        });
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/members/" + Recipient.ID + "-" + Recipient.Name.URLEscape() + ".html#vmessage" + ID;
        } 
    }
}
