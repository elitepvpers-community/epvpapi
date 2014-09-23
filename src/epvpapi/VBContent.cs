using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class VBContent
    {
        public string Code { get; set; }
        public object Value { get; set; }

        public virtual string Plain
        {
            get { return String.Format("[{0}]{1}[/{0}]", Code, Value); }
        }

        public VBContent(string code, object value)
        {
            Code = code;
            Value = value;
        }

        public class PlainTextContent : VBContent
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

            public override string Plain
            {
                get { return (string)Value; }
            }

            public PlainTextContent(string value) :
                base(null, value)
            { }
        }

        public class Spoiler : VBContent
        {
            public Spoiler(string value) :
                base("spoiler", value)
            { }
        }
    }
}
