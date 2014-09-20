using System;
using System.Collections.Generic;
using System.Globalization;
using epvpapi.Connection;

namespace epvpapi
{
    public class SocialGroup : UniqueObject, IDeletable, IUniqueWebObject
    {
        public enum Access
        {
            Public,
            Moderated,
            InviteOnly
        }

        [Flags]
        public enum Options
        {
            EnableGroupAlbums = 1,
            EnableGroupMessages = 2,
            ApproveGroupMessages = 4,
            JoinToView = 8,
            OnlyOwnerDiscussions = 10
        };

        private SocialGroup(uint id)
            : base(id)
        {
            Members = new List<User>();
            Maintainer = new User();
            Settings |= Options.EnableGroupAlbums | Options.EnableGroupMessages;
            AccessMode = Access.Public;
        }

        /// <summary>
        ///     Name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     URL to the group picture
        /// </summary>
        public string PictureUrl { get; set; }

        /// <summary>
        ///     Description of the group
        /// </summary>
        private string Description { get; set; }

        /// <summary>
        ///     All members of the group
        /// </summary>
        private List<User> Members { get; set; }

        /// <summary>
        ///     The group owner (maintainer)
        /// </summary>
        public User Maintainer { get; private set; }

        /// <summary>
        ///     Additional options that can be set
        /// </summary>
        private Options Settings { get; set; }

        /// <summary>
        ///     Access restrictions of the group determining who can see- or who can enter the group
        /// </summary>
        private Access AccessMode { get; set; }


        /// <summary>
        ///     Deletes the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        public void Delete<T>(ProfileSession<T> session) where T : User
        {
            if (session.User.GetHighestRank() < User.Rank.GlobalModerator
                && session.User != Maintainer)
                throw new InsufficientAccessException("You don't have enough access rights to delete this group");
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=delete",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "dodelete"),
                    new KeyValuePair<string, string>("groupid", Id.ToString()),
                    new KeyValuePair<string, string>("pictureid", String.Empty),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("url",
                        "http%3A%2F%2Fwww.elitepvpers.com%2Fforum%2Fgroups%2F" + Id + "--.html"),
                    new KeyValuePair<string, string>("confirm", "++Yes++")
                });
        }

        public string GetUrl()
        {
            return "http://www.elitepvpers.com/forum/groups/" + Id + "-" + Name.UrlEscape() + ".html";
        }

        /// <summary>
        ///     Creates a <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="name"> Name of the group </param>
        /// <param name="description"> Description of the group </param>
        /// <param name="access"> Access restrictions of the group determining who can see- or who can enter the group </param>
        /// <param name="settings"> Additional options that can be set  </param>
        /// <returns> The just created SocialGroup </returns>
        public static SocialGroup Create(Session session, string name, string description, Access access = Access.Public,
            Options settings = Options.EnableGroupAlbums | Options.EnableGroupMessages)
        {
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=docreate",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "docreate"),
                    new KeyValuePair<string, string>("groupid", String.Empty),
                    new KeyValuePair<string, string>("url", "http%3A%2F%2Fwww.elitepvpers.com%2Fforum%2Fgroups%2F"),
                    new KeyValuePair<string, string>("socialgroupcategoryid", "1"),
                    new KeyValuePair<string, string>("groupname", name),
                    new KeyValuePair<string, string>("groupdescription", description),
                    new KeyValuePair<string, string>("grouptype", access.ToString()),
                    new KeyValuePair<string, string>("options%5Benable_group_albums%5D",
                        (Convert.ToInt32(settings.HasFlag(Options.EnableGroupAlbums)).ToString())),
                    new KeyValuePair<string, string>("options%5Benable_group_messages%5D",
                        (Convert.ToInt32(settings.HasFlag(Options.EnableGroupMessages)).ToString()))
                });

            return new SocialGroup(0);
        }


        /// <summary>
        ///     Requests a ownership transfer for the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="user"> User that will be requested to be the new owner </param>
        public void RequestTransfer(Session session, User user)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=dotransfer&groupid=" + Id,
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "dotransfer"),
                    new KeyValuePair<string, string>("groupid", Id.ToString()),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("url", String.Empty),
                    new KeyValuePair<string, string>("targetusername", user.Name),
                });
        }


        /// <summary>
        ///     Accepts a pending ownership transfer if any for the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        public void AcceptTransfer(Session session)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/groups/" + Id + "--.html",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "doaccepttransfer"),
                    new KeyValuePair<string, string>("groupid", Id.ToString()),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("url",
                        "http://www.elitepvpers.com/forum/private.php?do=showpm&pmid=99999999"),
                    new KeyValuePair<string, string>("confirm", "Yes"),
                });
        }


        /// <summary>
        ///     Edits the <c>SocialGroup</c> and applies the given options
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="description"> Description of the group </param>
        /// <param name="access"> Access restrictions of the group determining who can see- or who can enter the group </param>
        /// <param name="settings"> Additional options that can be set  </param>
        public void Edit(Session session, string description, Access access, Options settings)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=doedit",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("do", "doedit"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("url", "http://www.elitepvpers.com/forum/groups/" + Id + "--.html"),
                    new KeyValuePair<string, string>("socialgroupcategoryid", "1"),
                    new KeyValuePair<string, string>("groupdescription", description),
                    new KeyValuePair<string, string>("grouptype", access.ToString()),
                    new KeyValuePair<string, string>("options[enable_group_albums]",
                        Convert.ToInt32(settings.HasFlag(Options.EnableGroupAlbums))
                            .ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("options[enable_group_messages]",
                        Convert.ToInt32(settings.HasFlag(Options.EnableGroupMessages))
                            .ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("options[owner_mod_queue]",
                        Convert.ToInt32(settings.HasFlag(Options.ApproveGroupMessages))
                            .ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("options[join_to_view]",
                        Convert.ToInt32(settings.HasFlag(Options.JoinToView)).ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("options[only_owner_discussions]",
                        Convert.ToInt32(settings.HasFlag(Options.OnlyOwnerDiscussions))
                            .ToString(CultureInfo.InvariantCulture)),
                });

            Description = description;
            AccessMode = access;
            Settings = settings;
        }


        /// <summary>
        ///     Kicks a <c>User</c> out of the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="user"> User to kick </param>
        public void Kick(Session session, User user)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=pendingmembers",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "kickmembers"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("ids[" + user.Id + "]",
                        user.Id.ToString(CultureInfo.InvariantCulture))
                });
        }


        /// <summary>
        ///     Invites another user to the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="user"> User to invite </param>
        public void Invite(Session session, User user)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=sendinvite",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "sendinvite"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("username", user.Name)
                });
        }

        /// <summary>
        ///     Cancels a pending invite to the <c>SocialGroup</c> for another user
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        /// <param name="user"> User whose invite will be cancelled </param>
        public void CancelInvite(Session session, User user)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=pendingmembers",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "cancelinvites"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("ids[" + user.Id + "]",
                        user.Id.ToString(CultureInfo.InvariantCulture))
                });
        }


        /// <summary>
        ///     Joins the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        public void Join(Session session)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=dojoin",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "dojoin"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("pictureid", String.Empty),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("url",
                        "http%3A%2F%2Fwww.elitepvpers.com%2Fforum%2Fgroups%2F" + Id + "--.hml"),
                    new KeyValuePair<string, string>("confirm", "++Yes++")
                });
        }


        /// <summary>
        ///     Leaves the <c>SocialGroup</c>
        /// </summary>
        /// <param name="session"> Session that is used for sending the request </param>
        public void Leave(Session session)
        {
            if (Id == 0) throw new ArgumentException("Group ID must not be zero");
            session.ThrowIfInvalid();

            session.Post("http://www.elitepvpers.com/forum/group.php?do=doleave",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("do", "doleave"),
                    new KeyValuePair<string, string>("groupid", Id.ToString(CultureInfo.InvariantCulture)),
                    new KeyValuePair<string, string>("pictureid", String.Empty),
                    new KeyValuePair<string, string>("s", String.Empty),
                    new KeyValuePair<string, string>("securitytoken", session.SecurityToken),
                    new KeyValuePair<string, string>("url",
                        "http%3A%2F%2Fwww.elitepvpers.com%2Fforum%2Fgroups%2F" + Id + "--.hml"),
                    new KeyValuePair<string, string>("confirm", "++Yes++")
                });
        }
    }
}