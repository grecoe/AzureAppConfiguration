namespace AppConfigCoreUtil.Models
{
    internal class ConfigurationIdentity
    {
        /// <summary>
        /// AppConfiguration Key
        /// </summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// AppConfiguration label.
        /// </summary>
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// App configuration content type.
        /// </summary>
        public string ContentType { get;set ; } = string.Empty;
        /// <summary>
        /// Underlying class property name to map to.
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
