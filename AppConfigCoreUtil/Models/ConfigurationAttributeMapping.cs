namespace AppConfigCoreUtil.Models
{
    using AppConfigCoreUtil.Attributes;

    public class ConfigAttributeMapping
    {
        public ConfigurationSectionAttribute? SectionAttribute { get; set; } = null;
        public PropertyConfigurationAttribute? SectionConfiguration { get; set; } = null;
        public Dictionary<string, PropertyConfigurationAttribute> AttributeMappings { get; set; } = new Dictionary<string, PropertyConfigurationAttribute>();
    }
}
