using System;

namespace CIXAPI
{
    public class Topic
    {
        private readonly UserTopicResultSetUserTopicsUserTopic _data;
        private readonly Forum _forum;
        private readonly int _messageID;

        public Topic(UserTopicResultSetUserTopicsUserTopic data, Forum forum)
        {
            _data = data;
            _forum = forum;
            _messageID = 0;
        }

        /// <summary>
        /// Return the forum object to which this topic belongs.
        /// </summary>
        public Forum Forum
        {
            get { return _forum; }
        }

        /// <summary>
        /// Return the name of this topic
        /// </summary>
        public string Name
        {
            get { return _data.Name; }
        }

        /// <summary>
        /// Return the ID of the current message in the topic which is first
        /// unread message.
        /// </summary>
        public int MessageID
        {
            get { return _messageID; }
        }

        /// <summary>
        /// Return the number of unread messages in this topic.
        /// </summary>
        public int Unread
        {
            get
            {
                int unreadCount;
                return Int32.TryParse(_data.UnRead, out unreadCount) ? unreadCount : 0;
            }
        }
    }
}
