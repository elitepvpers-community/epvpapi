using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class Message : UniqueRecord
    {
        /// <summary>
        /// Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public enum Settings
        {
            /// <summary>
            /// If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,
        }

        public User Sender { get; set; }

        public Content Content { get; set; }

        public Message(uint id = 0)
            : this(id, new Content())
        { }

        public Message(Content content)
            : this(0, content)
        { }

        public Message(uint id, Content content)
            : base(id)
        {
            Content = content;       
            Sender = new User();
        }
    }
}
