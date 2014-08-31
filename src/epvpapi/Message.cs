using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
    public abstract class Message : UniqueObject
    {
        /// <summary>
        /// Indicating additional options that can be set when posting a message 
        /// </summary>
        [Flags]
        public enum Options
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
        /// Content of the post
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Additional options that can be set when posting a message 
        /// </summary>
        public Options Settings { get; set; }

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
            Settings |= Options.ParseURL | Options.ShowSignature;
            Date = new DateTime();
        }
    }
}
