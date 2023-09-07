namespace AppConfigCoreUtil.Attributes
{
    /// <summary>
    /// An attribute used ONLY to be used on classes. 
    /// 
    /// The attribute identifies the global section name and individual properties
    /// contained within that are identified with PropertyConfigurationAttribute attribute.
    /// 
    /// For example, you have an object that identifies these fields for a CosmosDB
    /// Cosmos:ConnectionString
    /// Cosmos:Database
    /// 
    /// The property section name here would be "Cosmos"
    /// 
    /// Additionallly, if you have a class that encapsulates a single property
    /// 1. It must be content type application/json
    /// 2. The class is attributed with both ConfigurationSectionAttribute and PropertyConfigurationAttribute
    /// 3. Both attributes have an identical value for SectionName and Key, respectively. 
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
