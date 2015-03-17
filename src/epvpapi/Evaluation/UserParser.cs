using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace epvpapi.Evaluation
{
    internal class UserParser : TargetableParser<User>, IDocumentParser
    {
        internal class RankParser : TargetableParser<User>, INodeParser
        {
            public RankParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                // Fetch the user title badges. User which do not belong to any group or do not have got any badges will be lacking of the 'rank' element in their profile page
                if (coreNode == null) return;

                var rankNodes = new List<HtmlNode>(coreNode.ChildNodes.GetElementsByTagName("img")); // every rank badge got his very own 'img' element

                foreach (var node in rankNodes)
                {
                    if (!node.Attributes.Contains("src")) continue;

                    var parsedRank = new Usergroup();
                    if (Usergroup.FromUrl(node.Attributes["src"].Value, out parsedRank)) // 'src' holds the url to the rank image
                        Target.Usergroups.Add(parsedRank);
                }
            }
        }

        internal class SessionUserAboutParser : TargetableParser<User>, IDocumentParser
        {
            public SessionUserAboutParser(User target)
                : base(target)
            { }

            public void Execute(HtmlDocument document)
            {
                var biographyNode = document.GetElementbyId("profilefield_value_1");
                Target.Biography = (biographyNode != null) ? biographyNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                var locationNode = document.GetElementbyId("profilefield_value_2");
                Target.Location = (locationNode != null) ? locationNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                var interestsNode = document.GetElementbyId("profilefield_value_3");
                Target.Interests = (interestsNode != null) ? interestsNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                var occupationNode = document.GetElementbyId("profilefield_value_4");
                Target.Occupation = (occupationNode != null) ? occupationNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";

                var steamIdNode = document.GetElementbyId("profilefield_value_8");
                Target.SteamID = (steamIdNode != null) ? steamIdNode.SelectSingleNode("text()[1]").InnerText.Strip() : "";
            }
        }

        internal class AboutParser : TargetableParser<User>, INodeParser
        {
            public AboutParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                var rootNode = coreNode.SelectSingleNode("div[1]/ul[1]");
                if (rootNode == null) return;

                var aboutRootNode = rootNode.SelectSingleNode("li[1]/dl[1]");
                if (aboutRootNode != null)
                {
                    var aboutFieldNodes = new List<HtmlNode>(aboutRootNode.ChildNodes.GetElementsByTagName("dt"));
                    var aboutValueNodes = new List<HtmlNode>(aboutRootNode.ChildNodes.GetElementsByTagName("dd"));

                    if (aboutFieldNodes.Count == aboutValueNodes.Count)
                    {
                        foreach (var fieldNode in aboutFieldNodes)
                        {
                            var actualValue = aboutValueNodes.ElementAt(aboutFieldNodes.IndexOf(fieldNode)).InnerText;

                            if (fieldNode.InnerText == "Biography")
                                Target.Biography = actualValue;
                            else if (fieldNode.InnerText == "Location")
                                Target.Location = actualValue;
                            else if (fieldNode.InnerText == "Interests")
                                Target.Interests = actualValue;
                            else if (fieldNode.InnerText == "Occupation")
                                Target.Occupation = actualValue;
                            else if (fieldNode.InnerText == "Steam ID")
                                Target.SteamID = actualValue;
                        }
                    }
                }

                var signatureNode = rootNode.SelectSingleNode("li[2]/dl[1]/dd[1]");
                if(signatureNode != null)
                    new ContentParser(Target.Signature.Elements).Execute(signatureNode);
            }
        }


        internal class LastVisitorsParser : TargetableParser<User>, INodeParser
        {
            public LastVisitorsParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]/ol[1]");
                if (coreNode == null) return;

                Target.LastVisitors = new List<User>();
                foreach (var visitorNode in coreNode.ChildNodes.GetElementsByTagName("li"))
                {
                    var profileLinkNode = visitorNode.SelectSingleNode("a[1]");
                    if (profileLinkNode == null) continue;
                    var profileLink = (profileLinkNode.Attributes.Contains("href")) ? profileLinkNode.Attributes["href"].Value : "";

                    var userNameNode = profileLinkNode.SelectSingleNode("span[1]");
                    if (userNameNode == null) // non-ranked users got their name wrapped in the 'a' element
                        userNameNode = profileLinkNode;

                    Target.LastVisitors.Add(new User(userNameNode.InnerText, User.FromUrl(profileLink)));
                }
            }
        }

        internal class FriendListParser : TargetableParser<User>, INodeParser
        {
            public FriendListParser(User target) : 
                base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                var friendCountNode = coreNode.SelectSingleNode("../div[1]");
                if (friendCountNode != null)
                {
                    var match = Regex.Match(friendCountNode.InnerText, @"Showing [0-9]+ of ([0-9]+) Friend\(s\)");
                    if (match.Groups.Count > 1)
                        Target.Friends = match.Groups[1].Value.To<double>().To<uint>();
                }
            }
        }


        internal class LastActivityParser : TargetableParser<User>, INodeParser
        {
            public LastActivityParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode != null)
                {
                    var lastActivityDateNode = coreNode.SelectSingleNode("text()[2]");
                    string date = (lastActivityDateNode != null) ? lastActivityDateNode.InnerText.Strip() : String.Empty;

                    var lastActivityTimeNode = coreNode.SelectSingleNode("span[2]");
                    string time = (lastActivityTimeNode != null) ? lastActivityTimeNode.InnerText.Strip() : String.Empty;

                    Target.LastActivity = (date + " " + time).ToElitepvpersDateTime();
                }
                else
                {
                    Target.CurrentStatus = User.Status.Invisible;
                }
            }
        }

        internal class GeneralInfoParser : TargetableParser<User>, INodeParser
        {
            public GeneralInfoParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                var userStatusNode = coreNode.SelectSingleNode("h1[1]/img[1]");
                if (userStatusNode != null)
                {
                    if (userStatusNode.Attributes.Contains("src"))
                    {
                        var userStatusLink = userStatusNode.Attributes["src"].Value;
                        if (userStatusLink.Contains("invisible"))
                            Target.CurrentStatus = User.Status.Invisible;
                        else if (userStatusLink.Contains("offline"))
                            Target.CurrentStatus = User.Status.Offline;
                        else if (userStatusLink.Contains("online"))
                            Target.CurrentStatus = User.Status.Online;
                    }
                }

                var userTitleNode = coreNode.SelectSingleNode("h2[1]");
                Target.Title = (userTitleNode != null) ? userTitleNode.InnerText : "";

                var userNameNode = coreNode.SelectSingleNode("h1[1]/span[1]") ??
                    coreNode.SelectSingleNode("h1[1]/strike[1]") ?? // If the user is banned, the name is struck through
                    coreNode.SelectSingleNode("h1[1]"); // In case the user has no special color, the <span> element will be missing and no attributes are used
                                   
                if (userNameNode == null) return;
                Target.Name = userNameNode.InnerText.Strip();
                Target.Banned = (userNameNode.Name == "strike") ? true : false;

                if (userNameNode.Attributes.Contains("style"))
                {
                    var match = Regex.Match(userNameNode.Attributes["style"].Value, @"color:(\S+)");
                    if (match.Groups.Count > 1)
                        Target.Namecolor = match.Groups[1].Value;
                }   
            }
        }

        internal class StatisticsParser : TargetableParser<User>, INodeParser
        {
            public bool IsSessionUser { get; set; }

            public StatisticsParser(User target, bool isSessionUser = false)
                : base(target)
            {
                IsSessionUser = isSessionUser;
            }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]");

                // Loop through the fields since vBulletin sorts them dynamically according to rank and certain user settings
                foreach (var statisticsGroup in coreNode.ChildNodes.GetElementsByTagName("fieldset"))
                {
                    var legendCaption = statisticsGroup.SelectSingleNode("legend[1]").InnerText;

                    if (legendCaption == "Total Posts")
                    {
                        var postsNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        Target.Posts = (postsNode != null) ? (uint)postsNode.InnerText.To<double>() : 0;

                        var postsPerDayNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                        Target.PostsPerDay = (postsPerDayNode != null) ? postsPerDayNode.InnerText.To<double>() : 0;
                    }
                    else if (legendCaption == "Visitor Messages")
                    {
                        var visitorMessagesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        Target.VisitorMessages = (visitorMessagesNode != null) ? (uint)visitorMessagesNode.InnerText.To<double>() : 0;

                        var lastVisitorMessageNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                        Target.LastVisitorMessage = (lastVisitorMessageNode != null) ? lastVisitorMessageNode.InnerText.ToElitepvpersDateTime() : new DateTime();
                    }
                    else if (legendCaption == "Thanks Given")
                    {
                        var givenThanksNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        Target.ThanksGiven = (givenThanksNode != null) ? (uint)givenThanksNode.InnerText.To<double>() : 0;

                        // The received thanks count is stored within the span element and is trailed after the language dependent definition.
                        // Unlike other elements, the count is not seperated and therefore needs some regex in order to extract the count
                        var thanksReceivedNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[1]");
                        if (thanksReceivedNode != null)
                        {
                            Match match = Regex.Match(thanksReceivedNode.InnerText, "([0-9,]+)"); // language independent
                            if (match.Groups.Count > 1)
                                Target.ThanksReceived = (uint)match.Groups[1].Value.To<double>();
                        }
                    }
                    else if (legendCaption == "General Information")
                    {
                        HtmlNode recommendationsNode = null;
                        if (Target.CurrentStatus != User.Status.Invisible || IsSessionUser)
                            recommendationsNode = statisticsGroup.SelectSingleNode("ul[1]/li[3]/text()[1]");
                        else
                            recommendationsNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");

                        Target.Recommendations = (recommendationsNode != null) ? (uint)recommendationsNode.InnerText.To<double>() : 0;
                    }
                    else if (legendCaption == "User Notes")
                    {
                        var userNotesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        Target.UserNotes = (userNotesNode != null) ? (uint)userNotesNode.InnerText.To<double>() : 0;

                        var lastNoteDateNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[1]");
                        var lastNoteTimeNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[2]");

                        if (lastNoteDateNode != null && lastNoteTimeNode != null)
                            Target.LastUserNote = (lastNoteDateNode.InnerText + " " + lastNoteTimeNode.InnerText).ToElitepvpersDateTime();
                    }
                    else if (legendCaption.Contains("Blog -")) // users can specify their own blog name that is trailed behind the 'Blog -' string
                    {
                        var blogEntriesNode = statisticsGroup.SelectSingleNode("ul[1]/li[1]/text()[1]");
                        // skip the first 2 characters since the value always contains a leading ':' and whitespace 
                        Target.Blog.Entries = new List<BlogEntry>((blogEntriesNode != null) ? new string(blogEntriesNode.InnerText.Skip(2).ToArray()).To<int>() : 0);

                        var lastEntryDateNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/text()[2]");
                        string date = (lastEntryDateNode != null) ? lastEntryDateNode.InnerText.Strip() : "";

                        var lastEntryTimeNode = statisticsGroup.SelectSingleNode("ul[1]/li[2]/span[2]");
                        string time = (lastEntryTimeNode != null) ? lastEntryTimeNode.InnerText.Strip() : "";

                        Target.Blog.LastEntry = (date + " " + time).ToElitepvpersDateTime();
                    }
                }
            }
        }

        internal class MiniStatsParser : TargetableParser<User>, INodeParser
        {
            public MiniStatsParser(User target)
                : base(target)
            { }

            public void Execute(HtmlNode coreNode)
            {
                if (coreNode == null) return;

                coreNode = coreNode.SelectSingleNode("div[1]/table[1]/tr[1]");
                if (coreNode == null) return;

                var fieldsRootNode = coreNode.SelectSingleNode("td[1]/dl[1]");
                if (fieldsRootNode == null) return;

                var miniStatsNodes = new List<HtmlNode>(fieldsRootNode.ChildNodes.GetElementsByTagName("dt"));
                var miniStatsValueNodes = new List<HtmlNode>(fieldsRootNode.ChildNodes.GetElementsByTagName("dd"));

                if (miniStatsNodes.Count != miniStatsValueNodes.Count) return;

                // loop through the key nodes since they can also occur occasionally (depends on what the user selects to be shown in the profile and/or the rank)
                foreach (var keyNode in miniStatsNodes)
                {
                    var valueNode = miniStatsValueNodes[miniStatsNodes.IndexOf(keyNode)];
                    if (valueNode == null) continue;

                    if (keyNode.InnerText == "Join Date")
                    {
                        Target.JoinDate = valueNode.InnerText.ToElitepvpersDateTime();
                    }
                    else if (keyNode.InnerText.Contains("elite*gold"))
                    {
                        var eliteGoldValueNode = valueNode.SelectSingleNode("text()[1]");
                        Target.EliteGold = (eliteGoldValueNode != null) ? eliteGoldValueNode.InnerText.To<int>() : Target.EliteGold;
                    }
                    else if (keyNode.InnerText.Contains("The Black Market"))
                    {
                        var positiveRatingsNode = valueNode.SelectSingleNode("span[1]");
                        Target.TBMProfile.Ratings.Positive = (positiveRatingsNode != null) ? positiveRatingsNode.InnerText.To<uint>() : Target.TBMProfile.Ratings.Positive;

                        var neutralRatingsNode = valueNode.SelectSingleNode("text()[1]");
                        Target.TBMProfile.Ratings.Neutral = (neutralRatingsNode != null) ? new string(neutralRatingsNode.InnerText.Skip(1).Take(1).ToArray()).To<uint>() : Target.TBMProfile.Ratings.Neutral;

                        var negativeRatingsNode = valueNode.SelectSingleNode("span[2]");
                        Target.TBMProfile.Ratings.Negative = (negativeRatingsNode != null) ? negativeRatingsNode.InnerText.To<uint>() : Target.TBMProfile.Ratings.Negative;
                    }
                    else if (keyNode.InnerText.Contains("Mediations"))
                    {
                        var positiveNode = valueNode.SelectSingleNode("span[1]");
                        Target.TBMProfile.Mediations.Positive = (positiveNode != null) ? positiveNode.InnerText.To<uint>() : 0;

                        var neutralNode = valueNode.SelectSingleNode("text()[1]");
                        Target.TBMProfile.Mediations.Neutral = (neutralNode != null) ? neutralNode.InnerText.TrimStart('/').To<uint>() : 0;
                    }
                }

                var avatarNode = coreNode.SelectSingleNode("td[2]/img[1]");
                Target.AvatarUrl = (avatarNode != null) ? avatarNode.Attributes.Contains("src") ? avatarNode.Attributes["src"].Value : "" : "";
            }
        }

        public bool IsSessionUser { get; private set; }

        public UserParser(User target, bool isSessionUser) : base(target)
        {
            IsSessionUser = isSessionUser;
        }

        public void Execute(HtmlDocument document)
        {
            new GeneralInfoParser(Target).Execute(document.GetElementbyId("username_box"));
            new LastActivityParser(Target).Execute(document.GetElementbyId("last_online"));

            // In case the user is the logged in user, all fields are editable and therefore got his own ids. 
            if (IsSessionUser)
                new SessionUserAboutParser(Target).Execute(document);
            else // otherwise, fields are not owning an id
                new AboutParser(Target).Execute(document.GetElementbyId("collapseobj_aboutme"));

            new RankParser(Target).Execute(document.GetElementbyId("rank"));
            new StatisticsParser(Target, IsSessionUser).Execute(document.GetElementbyId("collapseobj_stats"));
            new MiniStatsParser(Target).Execute(document.GetElementbyId("collapseobj_stats_mini"));
            new LastVisitorsParser(Target).Execute(document.GetElementbyId("collapseobj_visitors"));
            new FriendListParser(Target).Execute(document.GetElementbyId("friends_list"));
        }
    }
}
