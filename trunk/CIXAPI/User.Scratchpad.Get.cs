namespace CIXAPI
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public partial class Scratchpad
    {

        private string countField;

        private ScratchpadMessagesMsg[] messagesField;

        /// <remarks/>
        public string Count
        {
            get { return countField; }
            set { countField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Msg", typeof (ScratchpadMessagesMsg), IsNullable = false)]
        public ScratchpadMessagesMsg[] Messages
        {
            get { return messagesField; }
            set { messagesField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    public partial class ScratchpadMessagesMsg
    {

        private string authorField;

        private string bodyField;

        private string dateTimeField;

        private string flagField;

        private string forumField;

        private string idField;

        private string readField;

        private string repliesField;

        private string replyToField;

        private string topicField;

        /// <remarks/>
        public string Author
        {
            get { return authorField; }
            set { authorField = value; }
        }

        /// <remarks/>
        public string Body
        {
            get { return bodyField; }
            set { bodyField = value; }
        }

        /// <remarks/>
        public string DateTime
        {
            get { return dateTimeField; }
            set { dateTimeField = value; }
        }

        /// <remarks/>
        public string Flag
        {
            get { return flagField; }
            set { flagField = value; }
        }

        /// <remarks/>
        public string Forum
        {
            get { return forumField; }
            set { forumField = value; }
        }

        /// <remarks/>
        public string ID
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        public string Read
        {
            get { return readField; }
            set { readField = value; }
        }

        /// <remarks/>
        public string Replies
        {
            get { return repliesField; }
            set { repliesField = value; }
        }

        /// <remarks/>
        public string ReplyTo
        {
            get { return replyToField; }
            set { replyToField = value; }
        }

        /// <remarks/>
        public string Topic
        {
            get { return topicField; }
            set { topicField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public partial class NewDataSet2
    {

        private Scratchpad[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Scratchpad")]
        public Scratchpad[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }
    }
}