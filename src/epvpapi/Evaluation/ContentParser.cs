using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    internal class ContentParser : TargetableParser<List<Content.Element>>, INodeParser
    {
        public ContentParser(List<Content.Element> target)
            : base(target)
        { }

        private static T ParseElement<T>(HtmlNode node) where T : Content.Element, new()
        {
            var childNodes = new List<Content.Element>();
            new ContentParser(childNodes).Execute(node);
            var parsedElement = new T()
            {
                Childs = childNodes
            };

            return parsedElement;
        }

        public void Execute(HtmlNode coreNode)
        {
            if (coreNode == null) return;

            foreach (var subNode in coreNode.ChildNodes)
            {
                switch (subNode.Name)
                {
                    case "b": { Target.Add(ParseElement<Content.Element.BoldText>(subNode)); break; }
                    case "i": { Target.Add(ParseElement<Content.Element.ItalicText>(subNode)); break; }
                    case "u": { Target.Add(ParseElement<Content.Element.UnderlinedText>(subNode)); break; }
                    case "STRIKE": { Target.Add(ParseElement<Content.Element.StruckThroughText>(subNode)); break; }
                    case "blockquote": { Target.Add(ParseElement<Content.Element.IndentedText>(subNode)); break; }
                    case "br": { Target.Add(new Content.Element.PlainText(Environment.NewLine)); break; }
                    case "a":
                        {
                            if (subNode.Attributes.Any(attribute => attribute.Name == "href"))
                            {
                                var childNodes = new List<Content.Element>();
                                new ContentParser(childNodes).Execute(subNode);
                                var parsedElement = new Content.Element.Link()
                                {
                                    Url = subNode.Attributes["href"].Value,
                                    Childs = childNodes
                                };

                                Target.Add(parsedElement);
                            }

                            break;
                        }
                    case "img":
                        {
                            if (subNode.Attributes.Any(attribute => attribute.Name == "src"))
                            {
                                var childNodes = new List<Content.Element>();
                                new ContentParser(childNodes).Execute(subNode);
                                var parsedElement = new Content.Element.Image()
                                {
                                    Url = subNode.Attributes["src"].Value,
                                    Childs = childNodes
                                };

                                Target.Add(parsedElement);
                            }

                            break;
                        }
                    case "div":
                        {
                            if (subNode.Attributes.Any(attribute => attribute.Name == "style" && attribute.Value == "margin:20px; margin-top:5px"))
                            {
                                var codeBoxNode = subNode.SelectSingleNode("pre[1]");
                                if (codeBoxNode != null)
                                {
                                    var codeBoxContent = String.Join("", codeBoxNode.ChildNodes.Select(childNode => childNode.InnerText));
                                    Target.Add(new Content.Element.Code() { Content = codeBoxContent });
                                }
                            }

                            break;
                        }
                    case "font":
                        {
                            var parsedFontSize = ParseElement<Content.Element.FontSize>(subNode);
                            if (subNode.Attributes.Any(attribute => attribute.Name == "size"))
                                parsedFontSize.Size = subNode.Attributes["size"].Value.To<uint>();

                            Target.Add(parsedFontSize);

                            break;
                        }
                    case "#text":
                        {
                            if (!String.IsNullOrEmpty(subNode.InnerText))
                                Target.Add(new Content.Element.PlainText()
                                {
                                    Value = subNode.InnerText
                                });

                            break;
                        }
                }

                switch (subNode.Class())
                {
                    case "align-center": { Target.Add(ParseElement<Content.Element.CenteredText>(subNode)); break; }
                    case "align-left": { Target.Add(ParseElement<Content.Element.LeftAlignedText>(subNode)); break; }
                    case "align-right": { Target.Add(ParseElement<Content.Element.RightAlignedText>(subNode)); break; }
                    case "align-justify": { Target.Add(ParseElement<Content.Element.JustifiedText>(subNode)); break; }
                    case "bbcode-quote":
                        {
                            var quoteNode = subNode.SelectSingleNode("table[1]/tr[1]/td[1]");
                            var quoteContentNode = quoteNode.SelectSingleNode("div[2]");
                            if (quoteContentNode == null) continue;
                            var quoteAuthorNode = quoteNode.SelectSingleNode("div[1]/strong[1]");

                            var parsedQuoteElement = ParseElement<Content.Element.Quote>(quoteContentNode);
                            parsedQuoteElement.Author = (quoteAuthorNode != null)
                                ? new User(quoteAuthorNode.InnerText)
                                : new User();

                            Target.Add(parsedQuoteElement);

                            break;
                        }

                    case "spoiler-coll":
                        {
                            var spoilerContentNode = subNode.SelectSingleNode("div[2]");
                            var spoilerTextNode = subNode.SelectSingleNode("div[1]/span[1]/a[1]/span[2]");
                            if (spoilerContentNode == null || spoilerTextNode == null) continue;

                            var parsedSpoilerElement = ParseElement<Content.Element.Spoiler>(spoilerContentNode);
                            parsedSpoilerElement.Title = spoilerTextNode.InnerText;
                            Target.Add(parsedSpoilerElement);

                            break;
                        }
                }
            }
        }
    }
}
