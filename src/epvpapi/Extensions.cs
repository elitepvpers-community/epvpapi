using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace epvpapi
{
    public static class Extensions
    {
        // -> http://stackoverflow.com/questions/249760/how-to-convert-unix-timestamp-to-datetime-and-vice-versa
        public static DateTime ToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }


        // -> http://stackoverflow.com/questions/10260255/getelementsbytagname-in-htmlagilitypack
        public static IEnumerable<HtmlNode> GetElementsByName(this HtmlNode parent, string name)
        {
            return parent.Descendants().Where(node => node.Name == name);
        }


        public static IEnumerable<HtmlNode> GetElementsByNameXHTML(this HtmlNode parent, string name)
        {
            List<HtmlNode> matchingNodes = new List<HtmlNode>();

            foreach(HtmlNode node in parent.Descendants())
            {
                if (node.Attributes.Contains("name"))
                {
                    if (node.Attributes["name"].Value == name)
                        matchingNodes.Add(node);
                }
            }

            return matchingNodes;
        }


        public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode baseNode, string tagName)
        {
            return baseNode.ChildNodes.Where(node => node.Name == tagName);
        }

        // -> http://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        public static List<List<T>> Split<T>(this List<T> source, uint elementsPerSplit)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / elementsPerSplit)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// Strips every sort of line breaks from the string. Also removes leading and trailing whitespaces 
        /// </summary>
        /// <param name="target"> String being stripped </param>
        /// <returns> Stripped string </returns>
        public static string Strip(this string target)
        {
            return Regex.Replace(target, @"(^ +)|(\r\n|\n|\r|\t)|( +)$", "");
        }
    }
}
