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

        private static IEnumerable<T> ParseText<T>(IEnumerable<HtmlNode> nodeCollection) where T : Content.Element, new()
        {
            foreach (var node in nodeCollection)
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(node);
                var parsedElement = new T()
                {
                    Value = node.InnerText.Strip(),
                    Childs = new List<Content.Element>(childNodes.Where(childNode => node.InnerText != childNode.Value))
                };

                yield return parsedElement;
            }
        }

        private static IEnumerable<T> ParseText<T>(HtmlNode coreNode) where T : Content.Element, new()
        {
            return ParseText<T>(new List<HtmlNode> { coreNode });
        }

        private static IEnumerable<T> ParseAttribute<T>(IEnumerable<HtmlNode> nodeCollection, string attributeName) where T : Content.Element, new()
        {
            foreach (var node in nodeCollection.Where(n => n.Attributes.Contains(attributeName)))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(node);
                var parsedElement = new T()
                {
                    Value = node.Attributes[attributeName].Value,
                    // Since every html tag provides a text node, we need to check whether the nodes are already covered as another elment
                    Childs = new List<Content.Element>(childNodes.Where(childNode => node.InnerText != childNode.Value)) 
                };

                yield return parsedElement;
            }
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

                foreach (var parsedQuoteElement in ParseText<Content.Element.Quote>(quoteContentNode))
                {
                    parsedQuoteElement.Author = (quoteAuthorNode != null)
                        ? new User(quoteAuthorNode.InnerText)
                        : new User();

                    Target.Add(parsedQuoteElement);
                }
            }

            // every code node got the specified style attributes
            foreach (var codeNode in coreNode.ChildNodes.GetElementsByAttribute("style", "margin:20px; margin-top:5px"))
                Target.AddRange(ParseText<Content.Element.GenericCode>(codeNode.ChildNodes
                    .GetElementsByAttribute("dir", "ltr")));

            
            // get all spoilers within the specified core node and extract the text in the spoiler
            foreach (var spoilerNode in coreNode.ChildNodes.GetElementsByClassName("spoiler-coll"))
            {
                var spoilerContentNode = spoilerNode.SelectSingleNode("div[2]");     
                var spoilerTextNode = spoilerNode.SelectSingleNode("div[1]/span[1]/a[1]/span[2]");
                if (spoilerContentNode == null || spoilerTextNode == null) continue;   

                foreach (var parsedSpoilerElement in ParseText<Content.Element.Spoiler>(spoilerContentNode))
                {
                    parsedSpoilerElement.Title = spoilerTextNode.InnerText;
                    Target.Add(parsedSpoilerElement);
                }
                
            }

            Target.AddRange(ParseText<Content.Element.BoldText>(coreNode.ChildNodes.GetElementsByTagName("b")));
            Target.AddRange(ParseText<Content.Element.ItalicText>(coreNode.ChildNodes.GetElementsByTagName("i")));
            Target.AddRange(ParseText<Content.Element.UnderlinedText>(coreNode.ChildNodes.GetElementsByTagName("u")));
            Target.AddRange(ParseText<Content.Element.StruckThroughText>(coreNode.ChildNodes.GetElementsByTagName("strike")));
            Target.AddRange(ParseText<Content.Element.CenteredText>(coreNode.ChildNodes.GetElementsByClassName("align-center")));
            Target.AddRange(ParseText<Content.Element.LeftAlignedText>(coreNode.ChildNodes.GetElementsByClassName("align-left")));
            Target.AddRange(ParseText<Content.Element.RightAlignedText>(coreNode.ChildNodes.GetElementsByClassName("align-right")));
            Target.AddRange(ParseText<Content.Element.JustifiedText>(coreNode.ChildNodes.GetElementsByClassName("align-justify")));
            Target.AddRange(ParseText<Content.Element.IndentedText>(coreNode.ChildNodes.GetElementsByTagName("blockquote")));
            Target.AddRange(ParseText<Content.Element.PlainText>(coreNode.ChildNodes.GetElementsByTagName("#text").Where(textNode => textNode.InnerText.Strip() != "")));
            // get all images within the specified core node and extract the url in the src attribute (image link)
            Target.AddRange(ParseAttribute<Content.Element.Image>(coreNode.ChildNodes.GetElementsByTagName("img"), "src"));
            // get all links within the specified core node and extract the url in the href attribute
            Target.AddRange(ParseAttribute<Content.Element.Link>(coreNode.ChildNodes.GetElementsByTagName("a"), "href"));
        }   
    }
}
