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
            public string Value { get; set; }
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

            public List<T> Filter<T>(string code) where T : Element
            {
                var concatenatedList = new List<T>();
                foreach (var child in Childs)
                {
                    if (child.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                        concatenatedList.Add(child as T);
                    concatenatedList.AddRange(child.Filter<T>(code));
                }

                return concatenatedList;
            }

            public class PlainText : Element
            {
                public override string Plain
                {
                    get { return (string) Value; }
                }

                public PlainText() :
                    this("")
                { }


                public PlainText(string value) :
                    base("", value)
                { }
            }

            public class ItalicText : Element
            {
                public ItalicText() :
                    this("")
                { }

                public ItalicText(string value) :
                    base("I", value)
                { }
            }

            public class UnderlinedText : Element
            {
                public UnderlinedText() :
                    this("")
                { }

                public UnderlinedText(string value) :
                    base("U", value)
                { }
            }

            public class BoldText : Element
            {
                public BoldText() :
                    this("")
                { }

                public BoldText(string value) :
                    base("B", value)
                { }
            }

            public class StruckThroughText : Element
            {
                public StruckThroughText() :
                    this("")
                { }

                public StruckThroughText(string value) :
                    base("STRIKE", value)
                { }
            }

            public class CenteredText : Element
            {
                public CenteredText() :
                    this("")
                { }

                public CenteredText(string value) :
                    base("CENTER", value)
                { }
            }

            public class LeftAlignedText : Element
            {
                public LeftAlignedText() :
                    this("")
                { }

                public LeftAlignedText(string value) :
                    base("LEFT", value)
                { }
            }

            public class RightAlignedText : Element
            {
                public RightAlignedText() :
                    this("")
                { }

                public RightAlignedText(string value) :
                    base("RIGHT", value)
                { }
            }

            public class JustifiedText : Element
            {
                public JustifiedText() :
                    this("")
                { }

                public JustifiedText(string value) :
                    base("JUSTIFY", value)
                { }
            }

            public class Spoiler : Element
            {
                public Spoiler() :
                    this("")
                { }

                public Spoiler(string value) :
                    base("spoiler", value)
                { }
            }

            public class Image : Element
            {
                public Image() :
                    this("")
                { }

                public Image(string value) :
                    base("img", value)
                { }
            }

            public class Link : Element
            {
                public Link() :
                    this("")
                { }

                public Link(string value) :
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

        public List<Element.PlainText> PlainTexts
        {
            get { return Filter<Element.PlainText>(""); }
        }

        public List<Element.Spoiler> Spoilers
        {
            get { return Filter<Element.Spoiler>("spoiler"); }
        }

        public List<Element.Quote> Quotes
        {
            get { return Filter<Element.Quote>("quote"); }
        }

        public List<Element.Image> Images
        {
            get { return Filter<Element.Image>("img"); }
        }

        public List<Element.Link> Links
        {
            get { return Filter<Element.Link>("url"); } 
        }

        public List<Element.BoldText> BoldText
        {
            get { return Filter<Element.BoldText>("B"); }
        }

        public List<Element.ItalicText> ItalicText
        {
            get { return Filter<Element.ItalicText>("I"); }
        }

        public List<Element.UnderlinedText> UnderlinedText
        {
            get { return Filter<Element.UnderlinedText>("U"); }
        }

        public List<Element.StruckThroughText> StruckThrough
        {
            get { return Filter<Element.StruckThroughText>("STRIKE"); }
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

        public List<T> Filter<T>(string code) where T : Element
        {
            var concatenatedList = new List<T>();
            foreach (var element in Elements)
            {
                if (element.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase))
                    concatenatedList.Add(element as T);
                concatenatedList.AddRange(element.Filter<T>(code));
            }

            return concatenatedList;
        }

        public override string ToString()
        {
            return String.Join(String.Empty, Elements.Select(content => content.Plain));
        }
    }
}
