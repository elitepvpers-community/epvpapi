using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        // -> http://stackoverflow.com/questions/10260255/getelementsbytagname-in-htmlagilitypack
        public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode parent, string name)
        {
            return parent.Descendants(name);
        }
    }
}
