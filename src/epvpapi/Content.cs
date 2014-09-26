using System;
using System.Collections.Generic;
using System.Linq;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
    public class Content 
    {
        /// <summary>
        /// Contents of the post
        /// </summary>
        public List<VBContentElement> Elements { get; set; }

        public List<VBContentElement> PlainTexts
        {
            get { return Filter(""); }
        }

        public List<VBContentElement> Spoilers
        {
            get { return Filter("spoiler"); }
        }

        public List<VBContentElement> Quotes
        {
            get { return Filter("quote"); }
        }

        public List<VBContentElement> Images
        {
            get { return Filter("img"); }
        }

        public Content(List<VBContentElement> elements)
        {
            Elements = elements;       
        }

        public Content():
            this(new List<VBContentElement>())
        { }

        public List<VBContentElement> Filter(string code)
        {
            return new List<VBContentElement>(Elements.Where(element => (element.Code == null)
                                                                         ? false
                                                                         : element.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)));
        }

        public override string ToString()
        {
            return String.Join(String.Empty, Elements.Select(content => content.Plain));
        }
    }
}
