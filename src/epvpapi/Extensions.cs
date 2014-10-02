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
        public static DateTime ToDateTime(this uint unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }


        /// <summary>
        /// Gets all child elements that are matching the specified name (attribute)
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="name"> Name attribute to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetElementsByName(this IEnumerable<HtmlNode> parent, string name)
        {
            return parent.Where(node => node.Name == name);
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
        public static IEnumerable<HtmlNode> GetElementsByNameXHtml(this IEnumerable<HtmlNode> parent, string name)
        {
            return  parent
                    .Where(node => node.Attributes.Contains("name"))
                    .Where(node => node.Attributes["name"].Value == name);
        }

        /// <summary>
        /// Gets all child elements of the specified tag name
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="tagName"> Tag name to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetElementsByTagName(this IEnumerable<HtmlNode> parent, string tagName)
        {
            return parent.Where(node => node.Name == tagName);
        }

        public static IEnumerable<HtmlNode> GetElementsByAttribute(this IEnumerable<HtmlNode> parent, string attributeName,
                                                                    string attributeValue)
        {
            return parent.Where(node => (node.Attributes.Contains(attributeName) ? node.Attributes[attributeName].Value == attributeValue : false));
        }

        /// <summary>
        /// Gets all child elements of the specified class name
        /// </summary>
        /// <param name="parent"> Node acting as the starting point </param>
        /// <param name="className"> class name to search for </param>
        /// <returns> <c>IEnumarable</c> of <c>HtmlNode</c>s that were found </returns>
        public static IEnumerable<HtmlNode> GetElementsByClassName(this IEnumerable<HtmlNode> parent, string className)
        {
            return parent.Where(node => node.Attributes
                                                   .Any(attribute => attribute.Name == "class" && attribute.Value == className));
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
            return Regex.Replace(target, @"(^ +)|(\r\n|\n|\r|\t) *|( +)$", "");
        }

        /// <summary>
        /// Gets unix timestamp (total seconds since 01.01.1970)
        /// </summary>
        /// <returns> Total seconds since 01.01.1970 </returns>
        public static uint UnixTimestamp()
        {
            return (uint) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        /// <summary>
        /// URL escapes a string
        /// </summary>
        /// <param name="target"> The string which has to become escaped </param>
        /// <returns> The escaped url string </returns>
        public static string UrlEscape(this string target)
        {
            // by default, all characters are escaped in links by vBulletin itself
            // the escaped string is rarely needed for unique addressing. it's just some type of placeholder that will be inserted in the address bar
            return (target != null) ? Regex.Replace(target, "([^a-zA-Z0-9]+)", "-").ToLower() : "-"; // return '-' by default to prevent the 403 not found errors.
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

        /// <summary>
        /// Turns a string into elitepvpers DateTime
        /// </summary>
        /// <param name="formattedTime"> The string containing the datetime </param>
        /// <returns> The elitepvpers DateTime</returns>
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

        /// <summary>
        /// Checks if a htmlNode got a specefic class
        /// </summary>
        /// <param name="node"></param>
        /// <param name="className"></param>
        /// <returns></returns>
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
        public static Target To<Target>(this object value) where Target : IConvertible
        {
            return (Target)Convert.ChangeType(value, typeof(Target), CultureInfo.InvariantCulture);
        }
    }
}
