namespace CIXAPI
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public partial class UserTopicResultSet
    {

        private string countField;

        private string startField;

        private UserTopicResultSetUserTopicsUserTopic[] userTopicsField;

        /// <remarks/>
        public string Count
        {
            get { return countField; }
            set { countField = value; }
        }

        /// <remarks/>
        public string Start
        {
            get { return startField; }
            set { startField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("UserTopic", typeof (UserTopicResultSetUserTopicsUserTopic),
            IsNullable = false)]
        public UserTopicResultSetUserTopicsUserTopic[] UserTopics
        {
            get { return userTopicsField; }
            set { userTopicsField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    public partial class UserTopicResultSetUserTopicsUserTopic
    {

        private string flagField;

        private string msgsField;

        private string nameField;

        private string statusField;

        private string unReadField;

        /// <remarks/>
        public string Flag
        {
            get { return flagField; }
            set { flagField = value; }
        }

        /// <remarks/>
        public string Msgs
        {
            get { return msgsField; }
            set { msgsField = value; }
        }

        /// <remarks/>
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        /// <remarks/>
        public string Status
        {
            get { return statusField; }
            set { statusField = value; }
        }

        /// <remarks/>
        public string UnRead
        {
            get { return unReadField; }
            set { unReadField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public partial class NewDataSet4
    {

        private UserTopicResultSet[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UserTopicResultSet")]
        public UserTopicResultSet[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }
    }

}