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

                // The actual message content is stored within several nodes. There may be different tags (such as <a> for links, linebreaks...)
                // This is why just all descendent text nodes are retrieved.
                var contentNodes = new List<HtmlNode>(coreNode.SelectSingleNode("div[2]").Descendants()
                                                                .Where(node => node.Name == "#text" && node.InnerText.Strip() != ""));
                contentNodes.ForEach(node => Target.Content += node.InnerText);
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
