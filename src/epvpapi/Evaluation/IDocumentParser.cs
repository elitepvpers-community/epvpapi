using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    public interface IDocumentParser
    {
        void Execute(HtmlDocument document);
    }
}

