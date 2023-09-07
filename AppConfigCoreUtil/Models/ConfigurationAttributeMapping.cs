namespace AppConfigCoreUtil.Models
{
    using AppConfigCoreUtil.Attributes;

    /// <summary>
    /// Encapsulates the configuration/attribute information on a class that is attributed with 
    /// ConfigurationSectionAttribute and/or PropertyConfigurationAttribute OR a class attributed 
    /// with ConfigurationSectionAttribute and has properties attributed with PropertyConfigurationAttribute.
    /// </summary>
    public class ConfigAttributeMapping
    {
        /// <summary>
        /// The section configuraiton attribute definiing the section name.
        /// </summary>
        public ConfigurationSectionAttribute? SectionAttribute { get; set; } = null;
        /// <summary>
        /// The PropertyConfigurationAttribute, only used when the class(section) is the full
        /// map to the Azure AppConfiguration property.
        /// </summary>
        public PropertyConfigurationAttribute? SectionConfiguration { get; set; } = null;
        /// <summary>
        /// Mapping for section names and properties contained within the section that should have 
        /// a notification request submitted when using a service. 
        /// </summary>
        public Dictionary<string, PropertyConfigurationAttribute> AttributeMappings { get; set; } = new Dictionary<string, PropertyConfigurationAttribute>();
    }
}
