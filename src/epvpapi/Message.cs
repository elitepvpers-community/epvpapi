using System;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
    public abstract class Message : UniqueWebObject
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
            ParseURL = 1,
        }

        /// <summary>
        /// Content of the post
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Date and time when the message was created
        /// </summary>
        public DateTime Date { get; set; }


        public Message(uint id)
            : this(id, null)
        { }

        public Message(string content)
            : this(0, content)
        { }

        public Message(uint id, string content)
            : base(0)
        {
            Content = content;       
            Date = new DateTime();
        }
    }
}
