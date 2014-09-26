using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    internal static class PrivateMessageParser
    {
        internal class ContentParser : TargetableParser<PrivateMessage>, INodeParser
        {
            public ContentParser(PrivateMessage target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                var titleNode = coreNode.SelectSingleNode("div[1]/strong[1]");
                Target.Title = (titleNode != null) ? titleNode.InnerText : "";

                // The actual message content is stored within the following div's child nodes. 
                // There may be different tags (such as <a> for links, linebreaks...) which require extra parsing
                var contentNode = coreNode.SelectSingleNode("div[2]");
                if (contentNode == null) return;

                // get all images within the private message
                var imageNodes = new List<HtmlNode>(contentNode.GetElementsByTagName("img").Where(imgNode => imgNode.Attributes.Contains("src")));

                // get only the plain text contents. Since every html tag provides a text node, we need to check whether the text nodes are already covered
                // as another elment
                var plainTextNodes = new List<HtmlNode>(contentNode.GetElementsByTagName("#text")
                                                        .Where(textNode =>
                                                        {
                                                            var strippedText = textNode.InnerText.Strip();
                                                            return (strippedText != ""
                                                                    && imageNodes.Any(imageNode => imageNode.Attributes["src"].Value != strippedText));
                                                        }));

                imageNodes.ForEach(imageNode => Target.Contents.Add(new VBContent.Image(imageNode.Attributes["src"].Value)));
                plainTextNodes.ForEach(plainTextNode => Target.Contents.Add(new VBContent.PlainText(plainTextNode.InnerText)));
            }
        }

        internal class SenderParser : TargetableParser<User>, INodeParser
        {
            public SenderParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;
                coreNode = coreNode.SelectSingleNode("tr[2]/td[1]/div[1]/a[1]");
                if (coreNode == null) return;

                var userNameNode = coreNode.SelectSingleNode("span[1]");

                Target.Name = (userNameNode != null) ? userNameNode.InnerText : "";
                Target.ID = (coreNode.Attributes.Contains("href")) ? User.FromURL(coreNode.Attributes["href"].Value) : 0;
            }
        }
    }
}
