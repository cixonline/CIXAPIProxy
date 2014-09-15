namespace CIXAPI
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://cixonline.com", IsNullable = false)]
    public sealed class ForumResultSet
    {

        private string countField;

        private string startField;

        private ForumResultSetForumsForumRow[] forumsField;

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
        [System.Xml.Serialization.XmlArrayItemAttribute("ForumRow", typeof (ForumResultSetForumsForumRow),
            IsNullable = false)]
        public ForumResultSetForumsForumRow[] Forums
        {
            get { return forumsField; }
            set { forumsField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17626")]
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://cixonline.com")]
    public sealed class ForumResultSetForumsForumRow
    {

        private string flagsField;

        private string nameField;

        private string priorityField;

        private string unreadField;

        /// <remarks/>
        public string Flags
        {
            get { return flagsField; }
            set { flagsField = value; }
        }

        /// <remarks/>
        public string Name
        {
            get { return nameField; }
            set { nameField = value; }
        }

        /// <remarks/>
        public string Priority
        {
            get { return priorityField; }
            set { priorityField = value; }
        }

        /// <remarks/>
        public string Unread
        {
            get { return unreadField; }
            set { unreadField = value; }
        }
    }
}