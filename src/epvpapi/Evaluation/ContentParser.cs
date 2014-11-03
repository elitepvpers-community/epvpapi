using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    internal class ContentParser : TargetableParser<List<Content.Element>>, INodeParser
    {
        public ContentParser(List<Content.Element> target)
            : base(target)
        { }

        private static IEnumerable<T> ParseElement<T>(IEnumerable<HtmlNode> nodeCollection) where T : Content.Element, new()
        {
            foreach (var node in nodeCollection)
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(node);
                var parsedElement = new T()
                {
                    Childs = childNodes
                };

                yield return parsedElement;
            }
        }

        private static IEnumerable<Content.Element.PlainText> ParseText(IEnumerable<HtmlNode> nodeCollection)
        {
            foreach (var node in nodeCollection)
            {
                var childNodes = new List<Content.Element>();
                var parsedElement = new Content.Element.PlainText()
                {
                    Value = node.InnerText.Strip()
                };

                yield return parsedElement;
            }
        }

        private static IEnumerable<T> ParseElement<T>(HtmlNode coreNode) where T : Content.Element, new()
        {
            return ParseElement<T>(new List<HtmlNode> { coreNode });
        }

        public void Execute(HtmlNode coreNode)
        {
            if (coreNode == null) return;

            // get all first layer child-quotes.
            var quoteNodes = new List<HtmlNode>(coreNode.ChildNodes
                .GetElementsByClassName("bbcode-quote")
                .Select(baseNode => baseNode.SelectSingleNode("table[1]/tr[1]/td[1]")));

            foreach (var quoteNode in quoteNodes)
            {
                var quoteContentNode = quoteNode.SelectSingleNode("div[2]");
                if (quoteContentNode == null) continue;
                var quoteAuthorNode = quoteNode.SelectSingleNode("div[1]/strong[1]");

                foreach (var parsedQuoteElement in ParseElement<Content.Element.Quote>(quoteContentNode))
                {
                    parsedQuoteElement.Author = (quoteAuthorNode != null)
                        ? new User(quoteAuthorNode.InnerText)
                        : new User();

                    Target.Add(parsedQuoteElement);
                }
            }

            // every code node got the specified style attributes
            foreach (var codeNode in coreNode.ChildNodes.GetElementsByAttribute("style", "margin:20px; margin-top:5px"))
                Target.AddRange(ParseElement<Content.Element.Code>(codeNode.ChildNodes
                    .GetElementsByAttribute("dir", "ltr")));

            
            // get all spoilers within the specified core node and extract the text in the spoiler
            foreach (var spoilerNode in coreNode.ChildNodes.GetElementsByClassName("spoiler-coll"))
            {
                var spoilerContentNode = spoilerNode.SelectSingleNode("div[2]");     
                var spoilerTextNode = spoilerNode.SelectSingleNode("div[1]/span[1]/a[1]/span[2]");
                if (spoilerContentNode == null || spoilerTextNode == null) continue;   

                foreach (var parsedSpoilerElement in ParseElement<Content.Element.Spoiler>(spoilerContentNode))
                {
                    parsedSpoilerElement.Title = spoilerTextNode.InnerText;
                    Target.Add(parsedSpoilerElement);
                }
            }

            // get all links within the specified core node and extract the url in the href attribute
            foreach (var linkNode in coreNode.ChildNodes.GetElementsByTagName("a").Where(n => n.Attributes.Contains("href")))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(linkNode);
                var parsedElement = new Content.Element.Link()
                {
                    Url = linkNode.Attributes["href"].Value,
                    Childs = childNodes
                };

                Target.Add(parsedElement);
            }

            // get all images within the specified core node and extract the url in the src attribute (image link)
            foreach (var imgNode in coreNode.ChildNodes.GetElementsByTagName("img").Where(n => n.Attributes.Contains("src")))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(imgNode);
                var parsedElement = new Content.Element.Image()
                {
                    Url = imgNode.Attributes["src"].Value,
                    Childs = childNodes
                };

                Target.Add(parsedElement);
            }

            Target.AddRange(ParseText(coreNode.ChildNodes.GetElementsByTagName("#text").Where(textNode => textNode.InnerText.Strip() != "")));
            Target.AddRange(ParseElement<Content.Element.BoldText>(coreNode.ChildNodes.GetElementsByTagName("b")));
            Target.AddRange(ParseElement<Content.Element.ItalicText>(coreNode.ChildNodes.GetElementsByTagName("i")));
            Target.AddRange(ParseElement<Content.Element.UnderlinedText>(coreNode.ChildNodes.GetElementsByTagName("u")));
            Target.AddRange(ParseElement<Content.Element.StruckThroughText>(coreNode.ChildNodes.GetElementsByTagName("strike")));
            Target.AddRange(ParseElement<Content.Element.CenteredText>(coreNode.ChildNodes.GetElementsByClassName("align-center")));
            Target.AddRange(ParseElement<Content.Element.LeftAlignedText>(coreNode.ChildNodes.GetElementsByClassName("align-left")));
            Target.AddRange(ParseElement<Content.Element.RightAlignedText>(coreNode.ChildNodes.GetElementsByClassName("align-right")));
            Target.AddRange(ParseElement<Content.Element.JustifiedText>(coreNode.ChildNodes.GetElementsByClassName("align-justify")));
            Target.AddRange(ParseElement<Content.Element.IndentedText>(coreNode.ChildNodes.GetElementsByTagName("blockquote")));
        }   
    }
}
