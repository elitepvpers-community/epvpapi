using System;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
    public abstract class Message : UniqueObject
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
        /// Contents of the post
        /// </summary>
        public List<VBContent> Contents { get; set; }

        public List<VBContent> PlainTexts
        {
            get { return Filter(""); }
        }

        public List<VBContent> Spoilers
        {
            get { return Filter("spoiler"); }
        }

        public List<VBContent> Quotes
        {
            get { return Filter("quote"); }
        }

        public List<VBContent> Images
        {
            get { return Filter("img"); }
        }


        /// <summary>
        /// Date and time when the message was created
        /// </summary>
        public DateTime Date { get; set; }


        public Message(uint id)
            : this(id, new List<VBContent>())
        { }

        public Message(List<VBContent> content)
            : this(0, content)
        { }

        public Message(uint id, List<VBContent> contents)
            : base(id)
        {
            Contents = contents;       
            Date = new DateTime();
        }

        public List<VBContent> Filter(string code)
        {
            return new List<VBContent>(Contents.Where(content => (content.Code == null)
                                                                 ? false 
                                                                 : content.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)));
        }

        public override string ToString()
        {
            return String.Join(String.Empty, Contents.Select(content => content.Plain));
        }
    }
}
