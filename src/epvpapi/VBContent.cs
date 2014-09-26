using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    public class VBContent
    {
        public string Code { get; set; }
        public virtual string Value { get; set; }

        public virtual string Plain
        {
            get { return String.Format("[{0}]{1}[/{0}]", Code, Value); }
        }

        public VBContent(string code = null, string value = null)
        {
            Code = code;
            Value = value;
        }

        public static bool TryParse(string input, out VBContent content)
        {
            content = new VBContent();
            var match = new Regex(@"(?:\[([a-zA-Z]+)\]){1}(.+)(?:\[\/\1\]){1}").Match(input);
            // 0 - everything, 1 - code, 2 - value
            if (match.Groups.Count != 3) return false;

            content = new VBContent(match.Groups[1].Value, match.Groups[2].Value);
            return true;
        }

        public class PlainText : VBContent
        {
            public override string Plain
            {
                get { return (string)Value; }
            }

            public PlainText(string value) :
                base("", value)
            { }
        }

        public class Spoiler : VBContent
        {
            public Spoiler(string value) :
                base("spoiler", value)
            { }
        }

        public class Image : VBContent
        {
            public Image(string value):
                base("img", value)
            { }
        }

        public class Quote : VBContent
        {
            public Message Message { get; set; }

            public override string Value
            {
                get { return Message.ToString(); }
            }

            public Quote(Message message) :
                base("quote")
            {
                Message = message; 
            }

            public Quote():
                this(new Message())
            { }
        }
    }
}
