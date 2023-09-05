namespace AppConfigCoreUtil.Attributes
{
    /// <summary>
    /// An attribute to decorate a class with identifying a section of AppConfiguration.
    /// 
    /// A section deliniates properties such as
    /// 
    /// section:prop1
    /// section:prop2
    /// </summary>
    [AttributeUsage(AttributeTargets.Class |
                    AttributeTargets.Struct,
                    AllowMultiple = false)
    ]
    public class ConfigurationSectionAttribute : Attribute
    {
        public string SectionName { get; set; }

        public ConfigurationSectionAttribute(string sectionName)
        {
            this.SectionName = sectionName;
        }
    }
}
