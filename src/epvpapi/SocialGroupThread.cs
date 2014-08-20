using epvpapi.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epvpapi
{
    public class SocialGroupThread : Thread, IReasonableDeletable
    {
        /// <summary>
        /// <c>SocialGroup</c> under which the thread is listed
        /// </summary>
        public SocialGroup SocialGroup { get; set; }

        /// <summary>
        /// List of all posts in the thread
        /// </summary>
        public List<SocialGroupPost> Posts { get; set; }

        public SocialGroupThread(uint id, SocialGroup socialGroup)
            : base(id)
        {
            SocialGroup = socialGroup;
            Posts = new List<SocialGroupPost>();
        }

        /// <summary>
        /// Creates a <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="socialGroup"> SocialGroup where to create the <c>SocialGroupThread</c></param>
        /// <param name="startPost"> Represents the content and title of the <c>SocialGroupThread</c> </param>
        /// <returns> Freshly created <c>SocialGroupThread</c></returns>
        public static SocialGroupThread Create(Session session, SocialGroup socialGroup, SocialGroupPost startPost)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=message",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("subject", startPost.Title),
                            new KeyValuePair<string, string>("message", startPost.Content),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("do", "message"),
                            new KeyValuePair<string, string>("gmid", String.Empty),
                            new KeyValuePair<string, string>("posthash", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("groupid", socialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("discussionid", String.Empty),
                            new KeyValuePair<string, string>("sbutton", "Nachricht+speichern"),
                            new KeyValuePair<string, string>("parseurl", Convert.ToInt32(startPost.Settings.HasFlag(Message.Options.ParseURL)).ToString()),
                            new KeyValuePair<string, string>("parseame", "1"),
                        });

            SocialGroupThread socialGroupThread = new SocialGroupThread(0, socialGroup) { Creator = session.User, Deleted = false };
            socialGroupThread.Posts.Insert(0, startPost);
            return socialGroupThread;
        }


        /// <summary>
        /// Deletes the <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="reason"> Reason for the deletion </param>
        public void Delete(Session session, string reason)
        {
            if (ID == 0) throw new System.ArgumentException("ID must not be empty");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group_inlinemod.php?gmids=",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("groupid", SocialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("messageids", ID.ToString()),
                            new KeyValuePair<string, string>("do", "doinlinedelete"),
                            new KeyValuePair<string, string>("url", "http://www.elitepvpers.com/forum/groups/" + SocialGroup.ID.ToString() + "--.html"),
                            new KeyValuePair<string, string>("inline_discussion", "1"),
                            new KeyValuePair<string, string>("deletetype", "1"),
                            new KeyValuePair<string, string>("deletereason", reason)
                        });
        }


        /// <summary>
        /// Replies to the <c>SocialGroupThread</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="post"> Reply to post </param>
        public void Reply(Session session, SocialGroupPost post)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=message",
                        new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("message", post.Content),
                            new KeyValuePair<string, string>("wysiwyg", "0"),
                            new KeyValuePair<string, string>("s", String.Empty),
                            new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                            new KeyValuePair<string, string>("do", "message"),
                            new KeyValuePair<string, string>("gmid", String.Empty),
                            new KeyValuePair<string, string>("posthash", String.Empty),
                            new KeyValuePair<string, string>("loggedinuser", session.User.ID.ToString()),
                            new KeyValuePair<string, string>("groupid", SocialGroup.ID.ToString()),
                            new KeyValuePair<string, string>("discussionid", ID.ToString()),
                            new KeyValuePair<string, string>("sbutton", "Post+Message"),
                            new KeyValuePair<string, string>("parseurl", (post.Settings & SocialGroupPost.Options.ParseURL).ToString()),
                            new KeyValuePair<string, string>("parseame", "1"),
                        });
        }
    }
}
