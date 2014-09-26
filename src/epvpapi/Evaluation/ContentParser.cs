using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    public class ContentParser : TargetableParser<List<Content.Element>>, INodeParser
    {
        public ContentParser(List<Content.Element> target)
            : base(target)
        { }


        public void Execute(HtmlNode coreNode)
        {
            var quoteNodes = new List<HtmlNode>(coreNode
                                                .GetElementsByClassName("bbcode-quote")
                                                .Select(baseNode => baseNode.SelectSingleNode("table[1]/tr[1]/td[1]")));

            foreach (var quoteNode in quoteNodes)
            {
                var quoteContentNode = quoteNode.SelectSingleNode("div[2]");
                if (quoteContentNode == null) continue;

                var quote = new Content.Element.Quote();
                new ContentParser(quote.Content.Elements).Execute(quoteContentNode);

                var quoteAuthorNode = quoteNode.SelectSingleNode("div[1]/strong[1]");
                quote.Author.Name = (quoteAuthorNode != null) ? quoteAuthorNode.InnerText : "";

                Target.Add(quote);
            }

            // get all images within the private message
            var imageNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("img").Where(imgNode => imgNode.Attributes.Contains("src")));
            imageNodes.ForEach(imageNode => Target.Add(new Content.Element.Image(imageNode.Attributes["src"].Value)));

            // get only the plain text contents. Since every html tag provides a text node, we need to check whether the text nodes are already covered
            // as another elment
            var plainTextNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("#text")
                                                    .Where(textNode =>
                                                    {
                                                        var strippedText = textNode.InnerText.Strip();
                                                        return (strippedText != "" && Target.All(content => content.Value != strippedText));
                                                    }));

            plainTextNodes.ForEach(plainTextNode => Target.Add(new Content.Element.PlainText(plainTextNode.InnerText)));
        }

       
    }
}
