﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi
{
    public interface IHtmlDocumentParseEngine
    {
        void Execute(HtmlDocument document);
    }
}
