using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace epvpapi
{
    public static class Extensions
    {
        // -> http://stackoverflow.com/questions/249760/how-to-convert-unix-timestamp-to-datetime-and-vice-versa
        public static DateTime ToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        /// <summary>
        /// Gets all descendent elements (unlimited levels) that are matching the specified name (attribute)
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="name"> Name attribute to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetDescendentElementsByName(this HtmlNode parent, string name)
        {
            return parent.Descendants().Where(node => node.Name == name);
        }

        /// <summary>
        /// Gets all child elements that are matching the specified name (attribute)
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="name"> Name attribute to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetElementsByName(this HtmlNode parent, string name)
        {
            return parent.ChildNodes.Where(node => node.Name == name);
        }

        /// <summary>
        /// Gets all descendent elements (unlimited levels) that are matching the specified name (attribute)
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="name"> Name attribute to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        /// <remarks>
        /// For XHTML use only
        /// </remarks>
        public static IEnumerable<HtmlNode> GetDescendentElementsByNameXHTML(this HtmlNode parent, string name)
        {
            return  parent.Descendants()
                    .Where(node => node.Attributes.Contains("name"))
                    .Where(node => node.Attributes["name"].Value == name);
        }

        /// <summary>
        /// Gets all child elements of the specified tag name
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="tagName"> Tag name to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode parent, string tagName)
        {
            return parent.ChildNodes.Where(node => node.Name == tagName);
        }

        /// <summary>
        /// Gets all descendent elements (unlimited levels) of the specified tag name
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="tagName"> Tag name to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetDescendentElementsByTagName(this HtmlNode parent, string tagName)
        {
            return parent.Descendants().Where(node => node.Name == tagName);
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
            return (target != null) ? Regex.Replace(target, @"(^ +)|(\r\n|\n|\r|\t)|( +)$", "") : null;
        }

        public static uint UnixTimestamp(this DateTime dateTime)
        {
            return (uint) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static string UrlEscape(this string target)
        {
            // by default, all characters are escaped in links by vBulletin itself
            return (target != null) ? Regex.Replace(target, "([^a-zA-Z0-9]+)", "-").ToLower() : null; 
        }

        /// <summary>
        /// (Raw) compares 2 <c>DateTime objects</c> and indicates wether both objects represent the same time (Year, Month, Day, Hour and Minute are compared)
        /// </summary>
        /// <param name="self"> <c>DateTime</c> being compared </param>
        /// <param name="other"> Other <c>DateTime</c> being compared </param>
        /// <returns> True if both objects represent the same time, false if not </returns>
        public static bool Compare(this DateTime self, DateTime other)
        {
            return (   (other.Year == self.Year) 
                    && (other.Month == self.Month) 
                    && (other.Day == self.Month) 
                    && (other.Hour == self.Hour) 
                    && (other.Minute == self.Minute) );
        }

        public static DateTime ToElitepvpersDateTime(this string formattedTime)
        {
            var commonFormats = new List<string>
            {
                "MM-dd-yyyy HH:mm",
                "MM/dd/yyyy HH:mm",
                "dd.MM.yyyy HH:mm",
                "dd.MM.yyyy, HH:mm",
                "MM/dd/yyyy, HH:mm",
                "MM/dd/yyyy",
                "dd/MM/yyyy",
                "MMM dd, yyyy - HH:mm"
            };

            formattedTime = formattedTime.Strip();
            if (formattedTime.Contains("Today"))
            {
                var index = formattedTime.IndexOf("Today");
                formattedTime = formattedTime.Remove(index, 5);
                formattedTime = formattedTime.Insert(index, DateTime.Now.Date.ToString("dd/MM/yyyy"));
            }

            var dateTime = new DateTime();
            foreach(var format in commonFormats)
            {
                if (DateTime.TryParseExact(formattedTime, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                    return dateTime;
            }

            return dateTime;
        }

        public static bool HasClass(this HtmlNode node, string className)
        {
            return node.Attributes.Any(attribute => attribute.Name == className);
        }

        /// <summary>
        /// Converts a string to an IConvertible (int, double, float, ..) object
        /// </summary>
        /// <typeparam name="T"> Type it will be converted to </typeparam>
        /// <param name="value"> The given string </param>
        /// <returns></returns>
        public static T To<T>(this string value) where T : IConvertible
        {
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
