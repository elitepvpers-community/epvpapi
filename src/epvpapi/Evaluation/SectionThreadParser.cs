using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    internal static class SectionThreadParser
    {
        internal class ReplyCreatorParser : TargetableParser<User>, INodeParser
        {
            private class StatisticParser : TargetableParser<User>, INodeParser
            {
                public StatisticParser(User target)
                    : base(target)
                { }

                public void Execute(HtmlNode coreNode)
                {
                    string innerText = coreNode.InnerText;

                    if (innerText.Contains("elite*gold"))
                    {
                        var elitegoldNode = coreNode.SelectSingleNode("text()[2]");
                        Target.EliteGold = (elitegoldNode != null)
                                                ? new String(elitegoldNode.InnerText.Skip(2).ToArray()).To<int>()
                                                : 0;

                    }
                    else if (innerText.Contains("The Black Market"))
                    {
                        var tbmPositiveNode = coreNode.SelectSingleNode("span[1]");
                        Target.TBMProfile.Ratings.Positive = (tbmPositiveNode != null)
                                                            ? tbmPositiveNode.InnerText.To<uint>()
                                                            : 0;

                        var tbmNeutralNode = coreNode.SelectSingleNode("text()[3]");
                        Target.TBMProfile.Ratings.Neutral = (tbmNeutralNode != null)
                                                        ? tbmNeutralNode.InnerText.TrimStart('/').TrimEnd('/').To<uint>()
                                                            : 0;

                        var tbmNegativeNode = coreNode.SelectSingleNode("span[2]");
                        Target.TBMProfile.Ratings.Negative = (tbmNegativeNode != null)
                                                            ? tbmNegativeNode.InnerText.To<uint>()
                                                            : 0;
                    }
                    else if (innerText.Contains("Mediations"))
                    {
                        var positiveNode = coreNode.SelectSingleNode("span[1]");
                        Target.TBMProfile.Mediations.Positive = (positiveNode != null) ? positiveNode.InnerText.To<uint>() : 0;

                        var neutralNode = coreNode.SelectSingleNode("text()[2]");
                        Target.TBMProfile.Mediations.Neutral = (neutralNode != null) ? neutralNode.InnerText.TrimStart('/').To<uint>() : 0;
                    }
                    else if (innerText.Contains("Join Date"))
                    {
                        // since the join date is formatted with the first 3 characters of the month + year, we'd need to use some regex here
                        // data may look as follows: Registriert seit: Jun 2007 (German)
                        var match = new Regex(@"([a-zA-Z]{3})\s{1}([0-9]+)").Match(coreNode.InnerText);
                        if (match.Groups.Count == 3)
                        {
                            for (var j = 1; j <= 12; j++)
                            {
                                if (CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(j).Contains(match.Groups[1].Value))
                                    Target.JoinDate = new DateTime(match.Groups[2].Value.To<int>(), j, 1);
                            }
                        }
                    }
                    else if (innerText.Contains("Posts"))
                    {
                        var match = new Regex("(?:Posts): ([0-9]+(?:.|,)[0-9]+)").Match(coreNode.InnerText);
                        if (match.Groups.Count > 1)
                            Target.Posts = (uint)double.Parse(match.Groups[1].Value);
                    }
                    else if (innerText.Contains("Received Thanks"))
                    {
                        var match = new Regex("(?:Received Thanks): ([0-9]+(?:.|,)[0-9]+)").Match(coreNode.InnerText);
                        if (match.Groups.Count > 1)
                            Target.ThanksReceived = (uint)double.Parse(match.Groups[1].Value);
                    }
                }
            }

            public ReplyCreatorParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                var postCreatorNode = coreNode.SelectSingleNode("div[1]/a[1]");
                if (postCreatorNode != null)
                {
                    Target.ID = postCreatorNode.Attributes.Contains("href")
                        ? User.FromUrl(postCreatorNode.Attributes["href"].Value)
                        : 0;

                    var userNameNode = postCreatorNode.SelectSingleNode("span[1]") ??
                                       postCreatorNode.SelectSingleNode("text()[1]");
                    Target.Name = (userNameNode != null) ? userNameNode.InnerText : "";

                    var userTitleNode = coreNode.SelectSingleNode("div[3]");
                    Target.Title = (userTitleNode != null) ? userTitleNode.InnerText : "";

                    new UserParser.RankParser(Target).Execute(coreNode.SelectSingleNode("div[4]"));

                    var userAvatarNode = coreNode.SelectSingleNode("div[5]/a[1]/img[1]");
                    if (userAvatarNode != null)
                        Target.AvatarUrl = userAvatarNode.Attributes.Contains("src")
                            ? userAvatarNode.Attributes["src"].Value
                            : "";

                    var additionalStatsNode = coreNode.SelectSingleNode("div[6]"); // node that contains posts, thanks, elite*gold, the join date...                    
                    if (additionalStatsNode != null)
                    {
                        var statsNodes = additionalStatsNode.ChildNodes.GetElementsByTagName("div");
                        if (statsNodes != null)
                        {
                            // Loop through the nodes and check for their descriptors since the 
                            // number of nodes depend on the user's rank and settings
                            foreach (var statsNode in statsNodes)
                                new StatisticParser(Target).Execute(statsNode);
                        }
                    }
                }
            }
        }
    }
}
