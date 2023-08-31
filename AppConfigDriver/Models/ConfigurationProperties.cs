namespace RPAppConfig.Models
{
    /// <summary>
    /// Settings from the appsettings.json.
    /// </summary>
    public class ConfigurationProperties
    {
        public string AzureAppConfigurationEndpoint { get; set; } = string.Empty;
        public string ActiveSettingsLabel { get; set; } = string.Empty;

        public string ModelLibrary { get; set; } = string.Empty;
    }
}
