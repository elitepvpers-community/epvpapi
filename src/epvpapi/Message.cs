using System;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
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

        public Message(int id = 0)
            : this(id, new Content())
        { }

        public Message(Content content)
            : this(0, content)
        { }

        public Message(int id, Content content)
            : base(id)
        {
            Content = content;
            Sender = new User();
        }
    }
}
