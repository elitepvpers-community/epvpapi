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

        public class PlainText : VBContent
        {
            public override string Plain
            {
                get { return (string)Value; }
            }

            public PlainText(string value) :
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
