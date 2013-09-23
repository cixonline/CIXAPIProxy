using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Xml;
using System.Xml.Serialization;

namespace CIXAPI
{
    public class Forums
    {
        private List<Forum> _forums;
        private bool _initialised;

        public Forums()
        {
            _initialised = false;
        }

        /// <summary>
        /// Return the total count of the number of forums that the user is
        /// joined to as seen by the server. Returns -1 if, for any reason, we
        /// can't get this count.
        /// </summary>
        public int Count
        {
            get {
                if (!_initialised)
                {
                    Initialise();
                }
                return (_forums != null ? _forums.Count : -1);
            }
        }

        /// <summary>
        /// Determine if the user is joined to the specified forum. Note that this works off the
        /// cached list rather than the server.
        /// </summary>
        /// <param name="forumName">The name of the forum to check</param>
        /// <returns>True if the user is joined to the forum, false otherwise.</returns>
        public bool IsJoined(string forumName)
        {
            if (!_initialised)
            {
                Initialise();
            }
            return _forums != null && _forums.Any(forum => forum.Name == forumName);
        }

        /// <summary>
        /// Join the specified forum.
        /// </summary>
        /// <param name="forumName">The name of the forum to join</param>
        /// <returns>True if we succeed, false if we fail for any reason (not found, locked, etc)</returns>
        public bool Join(string forumName)
        {
            string joinUrl = string.Format("cix.svc/forums/{0}/join.xml", forumName);

            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.GetUri(joinUrl));
            wrGeturl.Method = "GET";

            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (TextReader reader = new StreamReader(objStream))
                    {
                        string result = reader.ReadLine();
                        if (result != null && result.Contains("Success"))
                        {
                            _initialised = false; // Force re-load of the server forum list next time
                            return true;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("401"))
                {
                    throw new AuthenticationException("Authentication Failed", e);
                }
            }
            return false;
        }

        /// <summary>
        /// Initialise the forums element from the server.
        /// </summary>
        private void Initialise()
        {
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.GetUri("cix.svc/user/forums.xml"));
            wrGeturl.Method = "GET";

            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (XmlReader reader = XmlReader.Create(objStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ForumResultSet));
                        ForumResultSet listOfForums = (ForumResultSet)serializer.Deserialize(reader);

                        _forums = new List<Forum>();
                        foreach (ForumResultSetForumsForumRow forum in listOfForums.Forums)
                        {
                            Forum newForum = new Forum(forum);
                            _forums.Add(newForum);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("401"))
                {
                    throw new AuthenticationException("Authentication Failed", e);
                }
            }
            _initialised = true;
        }

        public Forum this[string forumName]
        {
            get
            {
                if (!_initialised)
                {
                    Initialise();
                }
                return _forums.FirstOrDefault(forum => forum.Name == forumName);
            }
        }

        /// <summary>
        /// Resign the specified forum.
        /// </summary>
        /// <param name="forumName">The name of the forum to resign</param>
        /// <returns>True if we succeed, false if we fail for any reason</returns>
        public bool Resign(string forumName)
        {
            string joinUrl = string.Format("cix.svc/forums/{0}/resign.xml", forumName);

            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.GetUri(joinUrl));
            wrGeturl.Method = "GET";

            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (TextReader reader = new StreamReader(objStream))
                    {
                        string result = reader.ReadLine();
                        if (result != null && result.Contains("Success"))
                        {
                            _initialised = false; // Force re-load of the server forum list next time
                            return true;
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Message.Contains("401"))
                {
                    throw new AuthenticationException("Authentication Failed", e);
                }
            }
            return false;
        }
    }
}
