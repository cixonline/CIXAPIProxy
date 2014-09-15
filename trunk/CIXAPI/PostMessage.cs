namespace CIXAPI
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public sealed class PostMessage
    {

        private string bodyField;

        private string forumField;

        private int msgIDField;

        private string topicField;

        /// <remarks/>
        public string Body
        {
            get { return bodyField; }
            set { bodyField = value; }
        }

        /// <remarks/>
        public string Forum
        {
            get { return forumField; }
            set { forumField = value; }
        }

        /// <remarks/>
        public int MsgID
        {
            get { return msgIDField; }
            set { msgIDField = value; }
        }

        /// <remarks/>
        public string Topic
        {
            get { return topicField; }
            set { topicField = value; }
        }
    }
}