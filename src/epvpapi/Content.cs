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
            public List<Element> Childs { get; set; } 

            public virtual string Plain
            {
                get { return String.Format("[{0}]{1}{2}[/{0}]", Code, Value, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
            }

            public Element(string code = null, string value = null)
            {
                Code = code;
                Value = value;
                Childs = new List<Element>();
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
                { }
            }

            public class ItalicText : Element
            {
                public ItalicText(string value) :
                    base("I", value)
                { }
            }

            public class UnderlinedText : Element
            {
                public UnderlinedText(string value) :
                    base("U", value)
                { }
            }

            public class BoldText : Element
            {
                public BoldText(string value) :
                    base("B", value)
                { }
            }

            public class StruckThrough : Element
            {
                public StruckThrough(string value):
                    base("STRIKE", value)
                { }
            }

            public class Spoiler : Element
            {
                public Spoiler(string value) :
                    base("spoiler", value)
                { }
            }

            public class Image : Element
            {
                public Image(string value) :
                    base("img", value)
                { }
            }

            public class Link : Element
            {
                public Link(string value):
                    base("url", value)
                { }
            }

            public class Quote : Element
            {
                public User Author { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}={1}]{2}{3}[/{0}]", Code, Author.Name, Value, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
                }

                public Quote(User author):
                    base("quote")
                {
                    Author = author;
                }

                public Quote() :
                    this(new User())
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

        public Content(string plainStringContent):
            this(new List<Element>() { new Element.PlainText(plainStringContent)} )
        { }

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
