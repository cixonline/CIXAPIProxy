namespace CIXAPI
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public sealed class TopicResultSet
    {

        private string countField;

        private string startField;

        private TopicResultSetTopicsTopic[] topicsField;

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
        [System.Xml.Serialization.XmlArrayItemAttribute("Topic", typeof (TopicResultSetTopicsTopic), IsNullable = false)
        ]
        public TopicResultSetTopicsTopic[] Topics
        {
            get { return topicsField; }
            set { topicsField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    public sealed class TopicResultSetTopicsTopic
    {

        private string descField;

        private string filesField;

        private string flagField;

        private string nameField;

        /// <remarks/>
        public string Desc
        {
            get { return descField; }
            set { descField = value; }
        }

        /// <remarks/>
        public string Files
        {
            get { return filesField; }
            set { filesField = value; }
        }

        /// <remarks/>
        public string Flag
        {
            get { return flagField; }
            set { flagField = value; }
        }

        /// <remarks/>
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }
    }
}