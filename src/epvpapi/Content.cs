using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace epvpapi
{
    /// <summary>
    /// Represents formatted vBulletin content
    /// </summary>
    public class Content 
    {
        /// <summary>
        /// Represents a content element such as spoilers, quotes, images, links...
        /// </summary>
        public class Element
        {
            /// <summary>
            /// Tag of the element that triggers the interpretation
            /// </summary>
            public string Tag { get; set; }
            public string Value { get; set; }

            /// <summary>
            /// Elements being wrapped by this element
            /// </summary>
            public List<Element> Childs { get; set; } 

            /// <summary>
            /// The plain representation that includes the element tag and the plain values of the child elements.
            /// Commonly used for posting content to the forum since vBulletin needs to interpret the raw content first
            /// </summary>
            public virtual string Plain
            {
                get { return String.Format("[{0}]{1}{2}[/{0}]", Tag, Value, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
            }

            public Element(string tag = null, string value = null)
            {
                Tag = tag;
                Value = value;
                Childs = new List<Element>();
            }

            /// <summary>
            /// Tries to parse the plain element code. For example, a spoiler may be formatted this way:
            /// [spoiler]This is a spoiler[/spoiler] where the code parses the tag (spoiler) and the value within the tags
            /// </summary>
            /// <param name="input"> Text to parse </param>
            /// <param name="contentElement"> Element representing the parsed results </param>
            /// <returns> true if the input was parsed, false if the input couldn't be parsed </returns>
            public static bool TryParse(string input, out Element contentElement)
            {
                contentElement = new Element();
                var match = new Regex(@"(?:\[([a-zA-Z]+)\]){1}(.+)(?:\[\/\1\]){1}").Match(input);
                // 0 - everything, 1 - Tag, 2 - value
                if (match.Groups.Count != 3) return false;

                contentElement = new Element(match.Groups[1].Value, match.Groups[2].Value);
                return true;
            }

            /// <summary>
            /// Filters all elements and child events by the given type
            /// </summary>
            /// <typeparam name="T"> Type of the element to parse deriving from <c>Element</c> </typeparam>
            /// <returns> List of all elements that matched the given tag within all child nodes </returns>
            public List<T> Filter<T>() where T : Element, new()
            {
                var filteringElement = new T();
                var concatenatedList = new List<T>();
                foreach (var child in Childs)
                {
                    if (child.Tag.Equals(filteringElement.Tag, StringComparison.InvariantCultureIgnoreCase))
                        concatenatedList.Add(child as T);
                    concatenatedList.AddRange(child.Filter<T>());
                }

                return concatenatedList;
            }

            /// <summary>
            /// Represents the default plain text
            /// </summary>
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

            /// <summary>
            /// Represents italic text
            /// </summary>
            public class ItalicText : Element
            {
                public ItalicText() :
                    this("")
                { }

                public ItalicText(string value) :
                    base("I", value)
                { }
            }

            /// <summary>
            /// Represents underlined text
            /// </summary>
            public class UnderlinedText : Element
            {
                public UnderlinedText() :
                    this("")
                { }

                public UnderlinedText(string value) :
                    base("U", value)
                { }
            }

            /// <summary>
            /// Represents bold text
            /// </summary>
            public class BoldText : Element
            {
                public BoldText() :
                    this("")
                { }

                public BoldText(string value) :
                    base("B", value)
                { }
            }

            /// <summary>
            /// Represents struckthrough text
            /// </summary>
            public class StruckThroughText : Element
            {
                public StruckThroughText() :
                    this("")
                { }

                public StruckThroughText(string value) :
                    base("STRIKE", value)
                { }
            }

            /// <summary>
            /// Represents centered text
            /// </summary>
            public class CenteredText : Element
            {
                public CenteredText() :
                    this("")
                { }

                public CenteredText(string value) :
                    base("CENTER", value)
                { }
            }

            /// <summary>
            /// Represents left aligned text
            /// </summary>
            public class LeftAlignedText : Element
            {
                public LeftAlignedText() :
                    this("")
                { }

                public LeftAlignedText(string value) :
                    base("LEFT", value)
                { }
            }

            /// <summary>
            /// Represents right aligned text
            /// </summary>
            public class RightAlignedText : Element
            {
                public RightAlignedText() :
                    this("")
                { }

                public RightAlignedText(string value) :
                    base("RIGHT", value)
                { }
            }

            /// <summary>
            /// Represents justified text
            /// </summary>
            public class JustifiedText : Element
            {
                public JustifiedText() :
                    this("")
                { }

                public JustifiedText(string value) :
                    base("JUSTIFY", value)
                { }
            }

            /// <summary>
            /// Represents indented text
            /// </summary>
            public class IndentedText : Element
            {
                public IndentedText() :
                    this("")
                { }

                public IndentedText(string value) :
                    base("INDENT", value)
                { }
            }

            /// <summary>
            /// Represents an expander that hides text on page refresh.
            /// </summary>
            public class Spoiler : Element
            {
                public string Title { get; set; }

                public Spoiler() :
                    this("")
                { }

                public Spoiler(string value) :
                    base("spoiler", value)
                { }
            }

            /// <summary>
            /// Represents an embedded image
            /// </summary>
            public class Image : Element
            {
                public Image() :
                    this("")
                { }

                public Image(string value) :
                    base("img", value)
                { }
            }

            /// <summary>
            /// Represents a reference to another url
            /// </summary>
            public class Link : Element
            {
                public Link() :
                    this("")
                { }

                public Link(string value) :
                    base("url", value)
                { }
            }

            /// <summary>
            /// Represents a formatted container for code hightling
            /// </summary>
            public class Code : Element
            {
                public Code() :
                    this("")
                { }

                public Code(string value) :
                    base("CODE", value)
                { }
            }


            public class Quote : Element
            {
                public User Author { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}={1}]{2}{3}[/{0}]", Tag, Author.Name, Value, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
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
        /// List of the formatted <c>Element</c>s representing the actual content
        /// </summary>
        public List<Element> Elements { get; set; }

        /// <summary>
        /// Retrieves all unformatted plain text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.PlainText> PlainTexts
        {
            get { return Filter<Element.PlainText>(); }
        }

        /// <summary>
        /// Retrieves all spoiler <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.Spoiler> Spoilers
        {
            get { return Filter<Element.Spoiler>(); }
        }

        /// <summary>
        /// Retrieves all quote <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.Quote> Quotes
        {
            get { return Filter<Element.Quote>(); }
        }

        /// <summary>
        /// Retrieves all image <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.Image> Images
        {
            get { return Filter<Element.Image>(); }
        }

        /// <summary>
        /// Retrieves all link <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.Link> Links
        {
            get { return Filter<Element.Link>(); } 
        }

        /// <summary>
        /// Retrieves all bold text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.BoldText> BoldText
        {
            get { return Filter<Element.BoldText>(); }
        }

        /// <summary>
        /// Retrieves all italic text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.ItalicText> ItalicText
        {
            get { return Filter<Element.ItalicText>(); }
        }

        /// <summary>
        /// Retrieves all underlined text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.UnderlinedText> UnderlinedText
        {
            get { return Filter<Element.UnderlinedText>(); }
        }

        /// <summary>
        /// Retrieves all struck through text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.StruckThroughText> StruckThroughText
        {
            get { return Filter<Element.StruckThroughText>(); }
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

        /// <summary>
        /// Filters all elements and child events by the given type
        /// </summary>
        /// <typeparam name="T"> Type of the element to parse deriving from <c>Element</c> </typeparam>
        /// <returns> List of all elements that matched the given tag within all child nodes </returns>
        public List<T> Filter<T>() where T : Element, new()
        {
            var filteringElement = new T();
            var concatenatedList = new List<T>();
            foreach (var element in Elements)
            {
                if (element.Tag.Equals(filteringElement.Tag, StringComparison.InvariantCultureIgnoreCase))
                    concatenatedList.Add(element as T);
                concatenatedList.AddRange(element.Filter<T>());
            }

            return concatenatedList;
        }

        /// <summary>
        /// Returns the plain text representation of the elements to be used in requests for transmitting content
        /// </summary>
        /// <returns> Plain text representation of the elements </returns>
        public override string ToString()
        {
            return String.Join(String.Empty, Elements.Select(content => content.Plain));
        }
    }
}
