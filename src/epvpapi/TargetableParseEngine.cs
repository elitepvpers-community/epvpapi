using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi
{
    public abstract class TargetableParseEngine<T>
    {
        public T Target { get; set; }

        public TargetableParseEngine(T target)
        {
            Target = target;
        }
    }
}
