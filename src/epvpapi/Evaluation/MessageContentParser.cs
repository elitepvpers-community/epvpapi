using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    public class MessageContentParser : TargetableParser<List<VBContent>>, INodeParser
    {
        public MessageContentParser(List<VBContent> target)
            : base(target)
        { }


        public void Execute(HtmlNode coreNode)
        {
            var quoteNodes = new List<HtmlNode>(coreNode
                                                .GetElementsByClassName("bbcode-quote")
                                                .Select(baseNode => baseNode.SelectSingleNode("table[1]/tr[1]/td[1]")));

            foreach (var quoteNode in quoteNodes)
            {
                var quote = new VBContent.Quote();
                new MessageContentParser(quote.Message.Contents).Execute(quoteNode.SelectSingleNode("div[2]"));
                Target.Add(quote);
            }

            // get all images within the private message
            var imageNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("img").Where(imgNode => imgNode.Attributes.Contains("src")));
            imageNodes.ForEach(imageNode => Target.Add(new VBContent.Image(imageNode.Attributes["src"].Value)));

            // get only the plain text contents. Since every html tag provides a text node, we need to check whether the text nodes are already covered
            // as another elment
            var plainTextNodes = new List<HtmlNode>(coreNode.GetElementsByTagName("#text")
                                                    .Where(textNode =>
                                                    {
                                                        var strippedText = textNode.InnerText.Strip();
                                                        return (strippedText != "" && Target.All(content => content.Value != strippedText));
                                                    }));

            plainTextNodes.ForEach(plainTextNode => Target.Add(new VBContent.PlainText(plainTextNode.InnerText)));
        }

       
    }
}
