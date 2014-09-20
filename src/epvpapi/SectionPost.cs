using System;
using System.Collections.Generic;
using epvpapi.Connection;

namespace epvpapi
{
    public class SectionPost : Post, IReportable, IUniqueWebObject
    {
        /// <summary>
        ///     Additional options that can be set when posting messages
        /// </summary>
        [Flags]
        public new enum Settings
        {
            /// <summary>
            ///     If set, all URLs in the message are going to be parsed
            /// </summary>
            ParseUrl = 1,

            /// <summary>
            ///     If set, the signature of the logged in user will be displayed beneath the message
            /// </summary>
            ShowSignature = 2
        }

        public SectionPost(string content = null, string title = null)
            : this(0, new SectionThread(new Section(0, "")), content, title)
        {
        }

        public SectionPost(uint id, SectionThread thread, string content = null, string title = null)
            : base(id, content, title)
        {
            Thread = thread;
        }

        /// <summary>
        ///     Icon associated with the post
        /// </summary>
        public short Icon { get; set; }

        private SectionThread Thread { get; set; }


        /// <summary>
        ///     Reports the <c>SectionPost</c> using the built-in report function
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason of the report </param>
        /// <remarks>
        ///     The ID of the <c>SectionPost</c> has to be given in order to report the post
        /// </remarks>
        public void Report(Session session, string reason)
        {
            if (Id == 0) throw new ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/report.php?do=sendemail",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("reason", reason),
                    new KeyValuePair<string, string>("postid", Id.ToString()),
                    new KeyValuePair<string, string>("do", "sendemail"),
                    new KeyValuePair<string, string>("url", "showthread.php?p=" + Id + "#post" + Id)
                });
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/joining-e-pvp/" + Thread.Id + "-" +
                   Thread.InitialPost.Title.UrlEscape() + ".html";
        }
    }
}