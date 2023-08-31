namespace AppConfigCoreUtil.Attributes
{
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
