namespace AppConfigCoreUtil.Models
{
    internal class ConfigurationIdentity
    {
        /// <summary>
        /// AppConfiguration full key identifying the property.
        /// </summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// AppConfiguration label for the property.
        /// </summary>
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// App configuration content type which should be one of AppConfigurationContentType,
        /// but other options are available, though not fully supported in the current iteration
        /// of this source.
        /// </summary>
        public string ContentType { get;set ; } = string.Empty;
        /// <summary>
        /// Underlying class property name in a class attributed by ConfigurationSectionAttribute and
        /// PropertyConfigurationAttribute.
        /// </summary>
        public string UnderlyingProperty { get; set; } = string.Empty;
    }

    /// <summary>
    /// Mapping of information for a specified object type.
    /// </summary>
    internal class ConfigurtionIdentities
    {
        /// <summary>
        /// True if the object has both a ConfigurationSectionAttribute and 
        /// a PropertyConfigurationAttribute indicating it is a single data 
        /// field in AppConfiguration that supports a class.
        /// </summary>
        public bool SectionAsProperty { get; set; } = false;
        /// <summary>
        /// A list of the mappings of individual fields that make up the property map between
        /// the AppConfiguration and underlying class object..
        /// </summary>
        public List<ConfigurationIdentity> IdentityList { get; set; } = new List<ConfigurationIdentity>();
    }
}
