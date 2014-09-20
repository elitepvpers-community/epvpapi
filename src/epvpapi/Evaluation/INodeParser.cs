using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    public interface INodeParser
    {
        void Execute(HtmlNode coreNode);
    }
}