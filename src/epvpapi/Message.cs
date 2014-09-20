using System;

namespace epvpapi
{
    /// <summary>
    ///     Base class for messages within the forum
    /// </summary>
    public abstract class Message : UniqueObject
    {
        /// <summary>
        ///     Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public enum Settings
        {
            /// <summary>
            ///     If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,
        }


        protected Message(string content)
            : this(0, content)
        {
        }

        protected Message(uint id, string content = null)
            : base(id)
        {
            Content = content;
            Date = new DateTime();
        }

        /// <summary>
        ///     Content of the post
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     Date and time when the message was created
        /// </summary>
        public DateTime Date { get; set; }
    }
}