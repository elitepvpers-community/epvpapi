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

                // strip the new line (\n) character from the message content 
                // that occurs in the beginning of each message in order to separate text from title 
                contentNode.InnerHtml = contentNode.InnerHtml.Strip(); 

                new Evaluation.ContentParser(Target.Content.Elements).Execute(contentNode);
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
                Target.ID = (coreNode.Attributes.Contains("href")) ? User.FromUrl(coreNode.Attributes["href"].Value) : 0;
            }
        }
    }
}
