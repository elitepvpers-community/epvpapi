using System;
using System.Collections.Generic;
using System.Linq;
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


        public void Execute(HtmlNode coreNode)
        {
            // get all first layer child-quotes.
            var quoteNodes = new List<HtmlNode>(coreNode
                                                .GetElementsByClassName("bbcode-quote")
                                                .Select(baseNode => baseNode.SelectSingleNode("table[1]/tr[1]/td[1]")));

            foreach (var quoteNode in quoteNodes)
            {
                var quoteContentNode = quoteNode.SelectSingleNode("div[2]");
                if (quoteContentNode == null) continue;
                var quoteAuthorNode = quoteNode.SelectSingleNode("div[1]/strong[1]");

                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(quoteContentNode);
                Target.Add(new Content.Element.Quote()
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => quoteNode.InnerText != node.Value)),
                    Author = (quoteAuthorNode != null) ? new User(quoteAuthorNode.InnerText) : new User()
                });

            }

            foreach(var boldTextNode in coreNode.GetElementsByTagName("b"))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(boldTextNode);
                Target.Add(new Content.Element.BoldText(boldTextNode.InnerText)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => boldTextNode.InnerText != node.Value))
                });
            }


            foreach (var italicTextNode in coreNode.GetElementsByTagName("i"))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(italicTextNode);
                Target.Add(new Content.Element.ItalicText(italicTextNode.InnerText)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => italicTextNode.InnerText != node.Value))
                });
            }

            foreach (var underlinedTextNode in coreNode.GetElementsByTagName("u"))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(underlinedTextNode);
                Target.Add(new Content.Element.UnderlinedText(underlinedTextNode.InnerText)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => underlinedTextNode.InnerText != node.Value))
                });
            }

            foreach (var struckThroughTextNode in coreNode.GetElementsByTagName("strike"))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(struckThroughTextNode);
                Target.Add(new Content.Element.StruckThrough(struckThroughTextNode.InnerText)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => struckThroughTextNode.InnerText != node.Value))
                });
            }

            // get all images within the specified core node and extract the url in the src attribute (image link)
            foreach (var imageNode in coreNode.GetElementsByTagName("img")
                                              .Where(imgNode => imgNode.Attributes.Contains("src")))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(imageNode);
                Target.Add(new Content.Element.Image(imageNode.Attributes["src"].Value)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => imageNode.InnerText != node.Value))
                });
            }

            // get all links within the specified core node and extract the url in the href attribute
            foreach (var linkNode in coreNode.GetElementsByTagName("a")
                                              .Where(linkNode => linkNode.Attributes.Contains("href")))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(linkNode);
                Target.Add(new Content.Element.Link(linkNode.Attributes["href"].Value.Strip())
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => linkNode.InnerText != node.Value))
                });
            }


            // get all spoilers within the specified core node and extract the text in the spoiler
            foreach (var spoilerNode in coreNode.GetElementsByClassName("spoiler-coll")
                                             .Select(spoilerBaseNode => spoilerBaseNode.SelectSingleNode("div[2]")))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(spoilerNode);
                Target.Add(new Content.Element.Spoiler(spoilerNode.InnerText)
                {
                    Childs = new List<Content.Element>(childNodes.Where(node => spoilerNode.InnerText != node.Value))
                });
            }

            // get only the plain text contents. Since every html tag provides a text node, we need to check whether the text nodes are already covered
            // as another elment
            foreach (var plainTextNode in coreNode.GetElementsByTagName("#text").Where(textNode => textNode.InnerText.Strip() != ""))
            {
                var childNodes = new List<Content.Element>();
                new ContentParser(childNodes).Execute(plainTextNode);
                Target.Add(new Content.Element.PlainText(plainTextNode.InnerText)
                {
                    Childs = childNodes
                });
            }
        }   
    }
}
