using System;
using System.Collections.Generic;
using System.Linq;

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
                get { return String.Format("[{0}]{1}[/{0}]", Tag, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
            }

            public Element(string tag = "") :
                this(tag, new List<Element>())
            { }

            public Element(string tag, List<Element> childs)
            {
                Tag = tag;
                Childs = childs;
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
                public string Value { get; set; }

                public override string Plain
                {
                    get { return (string)Value; }
                }

                public PlainText() :
                    this("")
                { }


                public PlainText(string value)
                {
                    Value = value;
                }
            }

            /// <summary>
            /// Represents italic text
            /// </summary>
            public class ItalicText : Element
            {
                public ItalicText() :
                    base("I")
                { }
            }

            /// <summary>
            /// Represents underlined text
            /// </summary>
            public class UnderlinedText : Element
            {
                public UnderlinedText() :
                    base("U")
                { }
            }

            /// <summary>
            /// Represents bold text
            /// </summary>
            public class BoldText : Element
            {
                public BoldText() :
                    base("B")
                { }
            }

            /// <summary>
            /// Represents struckthrough text
            /// </summary>
            public class StruckThroughText : Element
            {
                public StruckThroughText() :
                    base("STRIKE")
                { }
            }

            /// <summary>
            /// Represents centered text
            /// </summary>
            public class CenteredText : Element
            {
                public CenteredText() :
                    base("CENTER")
                { }
            }

            /// <summary>
            /// Represents left aligned text
            /// </summary>
            public class LeftAlignedText : Element
            {
                public LeftAlignedText() :
                    base("LEFT")
                { }
            }

            /// <summary>
            /// Represents right aligned text
            /// </summary>
            public class RightAlignedText : Element
            {
                public RightAlignedText() :
                    base("RIGHT")
                { }
            }

            /// <summary>
            /// Represents justified text
            /// </summary>
            public class JustifiedText : Element
            {
                public JustifiedText() :
                    base("JUSTIFY")
                { }
            }

            /// <summary>
            /// Represents indented text
            /// </summary>
            public class IndentedText : Element
            {
                public IndentedText() :
                    base("INDENT")
                { }
            }

            /// <summary>
            /// Represents an expander that hides text on page refresh.
            /// </summary>
            public class Spoiler : Element
            {
                public string Title { get; set; }

                public Spoiler() :
                    base("spoiler")
                { }
            }

            /// <summary>
            /// Represents an embedded image
            /// </summary>
            public class Image : Element
            {
                public string Url { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}]{1}[/{0}]", Tag, Url); }
                }

                public Image() :
                    base("img")
                { }
            }

            /// <summary>
            /// Represents a reference to another url
            /// </summary>
            public class Link : Element
            {
                public string Url { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}={1}]{2}[/{0}]", Tag, Url, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
                }

                public Link() :
                    base("url")
                { }
            }

            /// <summary>
            /// Represents a formatted container for code hightling
            /// </summary>
            public class Code : Element
            {
                public string Content { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}]{1}[/{0}]", Tag, Content); }
                }

                public Code() :
                    base("CODE")
                { }
            }

            /// <summary>
            /// Represents the container for formatting content to be displayed in another font size
            /// </summary>
            public class FontSize : Element
            {
                public uint Size { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}={1}]{2}[/{0}]", Tag, Size, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
                }

                public FontSize() :
                    base("SIZE")
                { }
            }

            /// <summary>
            /// Represents quote containers displaying information about the user and content being quoted
            /// </summary>
            public class Quote : Element
            {
                public User Author { get; set; }

                public override string Plain
                {
                    get { return String.Format("[{0}={1}]{2}[/{0}]", Tag, Author.Name, String.Join(String.Empty, Childs.Select(childContent => childContent.Plain))); }
                }

                public Quote(User author) :
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
        public List<Element.BoldText> BoldTexts
        {
            get { return Filter<Element.BoldText>(); }
        }

        /// <summary>
        /// Retrieves all italic text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.ItalicText> ItalicTexts
        {
            get { return Filter<Element.ItalicText>(); }
        }

        /// <summary>
        /// Retrieves all underlined text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.UnderlinedText> UnderlinedTexts
        {
            get { return Filter<Element.UnderlinedText>(); }
        }

        /// <summary>
        /// Retrieves all struck through text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.StruckThroughText> StruckThroughTexts
        {
            get { return Filter<Element.StruckThroughText>(); }
        }

        /// <summary>
        /// Retrieves all indented text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.IndentedText> IndentedTexts
        {
            get { return Filter<Element.IndentedText>(); }
        }

        /// <summary>
        /// Retrieves all centered text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.CenteredText> CenteredTexts
        {
            get { return Filter<Element.CenteredText>(); }
        }

        /// <summary>
        /// Retrieves all left aligned text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.LeftAlignedText> LeftAlignedTexts
        {
            get { return Filter<Element.LeftAlignedText>(); }
        }

        /// <summary>
        /// Retrieves all right aligned text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.RightAlignedText> RightAlignedTexts
        {
            get { return Filter<Element.RightAlignedText>(); }
        }

        /// <summary>
        /// Retrieves all justified text <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.JustifiedText> JustifiedTexts
        {
            get { return Filter<Element.JustifiedText>(); }
        }

        /// <summary>
        /// Retrieves all font size <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.FontSize> FontSizes
        {
            get { return Filter<Element.FontSize>(); }
        }

        /// <summary>
        /// Retrieves all code <c>Element</c>s contained by the <c>Elements</c> collection 
        /// </summary>
        public List<Element.Code> Codes
        {
            get { return Filter<Element.Code>(); }
        }

        public Content(List<Element> elements)
        {
            Elements = elements;
        }

        public Content(string plainStringContent) :
            this(new List<Element>() { new Element.PlainText(plainStringContent) })
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
