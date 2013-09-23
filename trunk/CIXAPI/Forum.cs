using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Xml;
using System.Xml.Serialization;

namespace CIXAPI
{
    public class Forum
    {
        private readonly ForumResultSetForumsForumRow _data;
        private Topics _topics;
        private bool _initialised;

        public Forum(ForumResultSetForumsForumRow data)
        {
            _data = data;
            _initialised = false;
        }

        /// <summary>
        /// Return the name of this forum.
        /// </summary>
        public string Name
        {
            get { return _data.Name; }
        }

        /// <summary>
        /// Return the set of topics for this forum.
        /// </summary>
        public Topics Topics
        {
            get
            {
                if (!_initialised)
                {
                    Initialise();
                }
                return _topics;
            }
        }

        /// <summary>
        /// Initialise the forums element from the server.
        /// </summary>
        private void Initialise()
        {
            string url = string.Format("cix.svc/user/{0}/topics.xml", _data.Name);
            WebRequest wrGeturl = WebRequest.Create(CIXOAuth.GetUri(url));
            wrGeturl.Method = "GET";

            try
            {
                Stream objStream = wrGeturl.GetResponse().GetResponseStream();
                if (objStream != null)
                {
                    using (XmlReader reader = XmlReader.Create(objStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(UserTopicResultSet));
                        UserTopicResultSet listOfTopics = (UserTopicResultSet)serializer.Deserialize(reader);

                        _topics = new Topics();
                        foreach (UserTopicResultSetUserTopicsUserTopic topic in listOfTopics.UserTopics)
                        {
                            Topic newTopic = new Topic(topic, this);
                            _topics.Add(newTopic);
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
    }
}
