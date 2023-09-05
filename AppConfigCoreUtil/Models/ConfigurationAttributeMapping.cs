namespace AppConfigCoreUtil.Models
{
    using AppConfigCoreUtil.Attributes;

    /// <summary>
    /// Class used to map attributes of a class to an attribute of AppConfiguration.
    /// </summary>
    public class ConfigAttributeMapping
    {
        public ConfigurationSectionAttribute? SectionAttribute { get; set; } = null;
        public PropertyConfigurationAttribute? SectionConfiguration { get; set; } = null;
        public Dictionary<string, PropertyConfigurationAttribute> AttributeMappings { get; set; } = new Dictionary<string, PropertyConfigurationAttribute>();
    }
}
