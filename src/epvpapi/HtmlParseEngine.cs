using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public abstract class HtmlParseEngine<T>
    {
        public T Target { get; set; }

        public HtmlParseEngine(T target)
        {
            Target = target;
        }

        public abstract void Execute();
    }
}
