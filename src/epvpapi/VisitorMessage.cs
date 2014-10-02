using epvpapi.Connection;
using System;
using System.Collections.Generic;

namespace epvpapi
{
    /// <summary>
    /// Represents a message on the profile of an <c>User</c>
    /// </summary>
    public class VisitorMessage : UnicastMessage, IUniqueWebObject
    {
        public VisitorMessage(uint id = 0)
            : this(id, new User(), new Content())
        { }
       
        public VisitorMessage(User receiver, Content content)
            : this(0, receiver, content)
        { }

        public VisitorMessage(uint id, User receiver, Content content)
            : base(id, receiver, content)
        { }

        /// <summary>
        /// Sends a <c>VisitorMessage</c> using the given session
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="settings"> Additional options that can be set </param>
        /// <remarks>
        /// The ID of the recipient has to be given in order to send the message
        /// </remarks>
        public void Send<TUser>(Session<TUser> session, Settings settings = Settings.ParseUrl) where TUser : User
        {
            if (Receiver.ID == 0) throw new ArgumentException("Receiver ID must not be empty");
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
                            new KeyValuePair<string, string>("u", Receiver.ID.ToString()),
                            new KeyValuePair<string, string>("u2", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", String.Empty),
                            new KeyValuePair<string, string>("parseurl", (settings & Settings.ParseUrl).ToString()),
                            new KeyValuePair<string, string>("lastcomment", "1381528657"),
                            new KeyValuePair<string, string>("allow_ajax_qc", "1"),
                            new KeyValuePair<string, string>("fromconverse", String.Empty),
                            new KeyValuePair<string, string>("message", Content.Elements.ToString()),
                        });
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/members/" + Receiver.ID + "-" + Receiver.Name.UrlEscape() + ".html#vmessage" + ID;
        } 
    }
}
