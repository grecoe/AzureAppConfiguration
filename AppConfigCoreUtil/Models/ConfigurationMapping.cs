namespace AppConfigCoreUtil.Models
{
    /// <summary>
    /// The list of fields in which notification is required while using a Service
    /// to recieve automatic notifications. 
    /// </summary>
    public class SectionConfiguration
    {
        /// <summary>
        /// Gets or sets the list of field names that request notification.
        /// </summary>
        public List<string> NotificationFields { get; set; } = new List<string>();

    }

    /// <summary>
    /// Configuration mapping available when using the AzureAppConfiguration class in
    /// a service. The data here is used with the AzureAppConfigurationOptions set up 
    /// for the service to provide sections to scan and fields to be notified on changes. 
    /// </summary>
    public class ConfigurationMapping
    {
        /// <summary>
        /// The list of configuration settings for an AppConfiguration section.
        /// 
        /// Key: Section Name
        /// Value: SectionConfiguration object containing field names in which to be notified.
        /// </summary>
        public Dictionary<string, SectionConfiguration> SectionMappings { get; set; } = new Dictionary<string, SectionConfiguration>();
    }
}
