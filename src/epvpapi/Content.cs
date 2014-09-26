using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace epvpapi
{
    /// <summary>
    /// Base class for messages within the forum
    /// </summary>
    public class Content 
    {
        public class Element
        {
            public string Code { get; set; }
            public virtual string Value { get; set; }

            public virtual string Plain
            {
                get { return String.Format("[{0}]{1}[/{0}]", Code, Value); }
            }

            public Element(string code = null, string value = null)
            {
                Code = code;
                Value = value;
            }

            public static bool TryParse(string input, out Element contentElement)
            {
                contentElement = new Element();
                var match = new Regex(@"(?:\[([a-zA-Z]+)\]){1}(.+)(?:\[\/\1\]){1}").Match(input);
                // 0 - everything, 1 - code, 2 - value
                if (match.Groups.Count != 3) return false;

                contentElement = new Element(match.Groups[1].Value, match.Groups[2].Value);
                return true;
            }

            public class PlainText : Element
            {
                public override string Plain
                {
                    get { return (string) Value; }
                }

                public PlainText(string value) :
                    base("", value)
                {
                }
            }

            public class Spoiler : Element
            {
                public Spoiler(string value) :
                    base("spoiler", value)
                {
                }
            }

            public class Image : Element
            {
                public Image(string value) :
                    base("img", value)
                {
                }
            }

            public class Quote : Element
            {
                public User Author { get; set; }
                public Content Content { get; set; }

                public override string Value
                {
                    get { return Content.ToString(); }
                }

                public Quote(Content content):
                    this(content, new User())
                { }

                public Quote(Content content, User author):
                    base("quote")
                {
                    Content = content;
                    Author = author;
                }

                public Quote() :
                    this(new Content())
                { }
            }
        }

        /// <summary>
        /// Contents of the post
        /// </summary>
        public List<Element> Elements { get; set; }

        public List<Element> PlainTexts
        {
            get { return Filter(""); }
        }

        public List<Element> Spoilers
        {
            get { return Filter("spoiler"); }
        }

        public List<Element> Quotes
        {
            get { return Filter("quote"); }
        }

        public List<Element> Images
        {
            get { return Filter("img"); }
        }

        public Content(List<Element> elements)
        {
            Elements = elements;       
        }

        public Content() :
            this(new List<Element>())
        { }

        public List<Element> Filter(string code)
        {
            return new List<Element>(Elements.Where(element => (element.Code == null)
                                                                         ? false
                                                                         : element.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)));
        }

        public override string ToString()
        {
            return String.Join(String.Empty, Elements.Select(content => content.Plain));
        }
    }
}
