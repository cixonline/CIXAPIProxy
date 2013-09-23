using System.Collections.Generic;
using System.Linq;

namespace CIXAPI
{
    public class Topics
    {
        private readonly List<Topic> _topics = new List<Topic>();

        public void Add(Topic newTopic)
        {
            _topics.Add(newTopic);
        }

        public int Count
        {
            get
            {
                return _topics.Count;
            }
        }

        public bool Contains(string topicName)
        {
            return _topics.Any(topic => topic.Name == topicName);
        }

        public Topic this[int i]
        {
            get
            {
                return _topics[i];
            }
        }

        public Topic this[string topicName]
        {
            get
            {
                return _topics.FirstOrDefault(topic => topic.Name == topicName);
            }
        }
    }
}
