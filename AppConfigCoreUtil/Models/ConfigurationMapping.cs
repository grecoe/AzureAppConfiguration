namespace AppConfigCoreUtil.Models
{
    public class SectionConfiguration
    {
        public List<string> NotificationFields { get; set; } = new List<string>();

    }

    public class ConfigurationMapping
    {
        public Dictionary<string, SectionConfiguration> SectionMappings { get; set; } = new Dictionary<string, SectionConfiguration>();
    }
}
