using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    public class MessageContentParser : TargetableParser<List<VBContentElement>>, INodeParser
    {
        public MessageContentParser(List<VBContentElement> target)
            : base(target)
        { }


        public void Execute(HtmlNode coreNode)
        {
            var quoteNodes = new List<HtmlNode>(coreNode
                                                .GetElementsByClassName("bbcode-quote")
                                                .Select(baseNode => baseNode.SelectSingleNode("table[1]/tr[1]/td[1]")));

            foreach (var quoteNode in quoteNodes)
            {
                var quote = new VBContentElement.Quote();
                new MessageContentParser(quote.Content.Elements).Execute(quoteNode.SelectSingleNode("div[2]"));
                Target.Add(quote);
            }

            // get all images within the private message
            var imageNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("img").Where(imgNode => imgNode.Attributes.Contains("src")));
            imageNodes.ForEach(imageNode => Target.Add(new VBContentElement.Image(imageNode.Attributes["src"].Value)));

            // get only the plain text contents. Since every html tag provides a text node, we need to check whether the text nodes are already covered
            // as another elment
            var plainTextNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("#text")
                                                    .Where(textNode =>
                                                    {
                                                        var strippedText = textNode.InnerText.Strip();
                                                        return (strippedText != "" && Target.All(content => content.Value != strippedText));
                                                    }));

            plainTextNodes.ForEach(plainTextNode => Target.Add(new VBContentElement.PlainText(plainTextNode.InnerText)));
        }

       
    }
}
