namespace AppConfigCoreUtil.Models
{
    /// <summary>
    /// The list of fields in which notification is required.
    /// </summary>
    public class SectionConfiguration
    {
        public List<string> NotificationFields { get; set; } = new List<string>();

    }

    /// <summary>
    /// Configuration mapping for a class to AppConfiguration properties.
    /// </summary>
    public class ConfigurationMapping
    {
        public Dictionary<string, SectionConfiguration> SectionMappings { get; set; } = new Dictionary<string, SectionConfiguration>();
    }
}
