using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    public class VBContentElement
    {
        public string Code { get; set; }
        public virtual string Value { get; set; }

        public virtual string Plain
        {
            get { return String.Format("[{0}]{1}[/{0}]", Code, Value); }
        }

        public VBContentElement(string code = null, string value = null)
        {
            Code = code;
            Value = value;
        }

        public static bool TryParse(string input, out VBContentElement contentElement)
        {
            contentElement = new VBContentElement();
            var match = new Regex(@"(?:\[([a-zA-Z]+)\]){1}(.+)(?:\[\/\1\]){1}").Match(input);
            // 0 - everything, 1 - code, 2 - value
            if (match.Groups.Count != 3) return false;

            contentElement = new VBContentElement(match.Groups[1].Value, match.Groups[2].Value);
            return true;
        }

        public class PlainText : VBContentElement
        {
            public override string Plain
            {
                get { return (string)Value; }
            }

            public PlainText(string value) :
                base("", value)
            { }
        }

        public class Spoiler : VBContentElement
        {
            public Spoiler(string value) :
                base("spoiler", value)
            { }
        }

        public class Image : VBContentElement
        {
            public Image(string value):
                base("img", value)
            { }
        }

        public class Quote : VBContentElement
        {
            public Content Content { get; set; }

            public override string Value
            {
                get { return Content.ToString(); }
            }

            public Quote(Content content) :
                base("quote")
            {
                Content = content; 
            }

            public Quote():
                this(new Content())
            { }
        }
    }
}
