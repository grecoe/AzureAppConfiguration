namespace AppConfigModelLib.Models.Sections
{
    using AppConfigCoreUtil.Attributes;

    /// <summary>
    /// To have a single property in AzureAppConfiguration which is a JSON object, you must
    /// set both the ConfigurationSection and PropertyConfiguration to the class, which both
    /// have the same name. 
    /// 
    /// For embedded objects you MUST use application/jdon in the configuration here as 
    /// well as it's type in the AppConfiguration itself. 
    /// </summary>
    [ConfigurationSection("SingleProp:Data")]
    [PropertyConfiguration(
            "SingleProp:Data",
            contentType: AppConfigurationContentType.CONTENT_JSON,
            notify: true)]
    public class SinglePropConfiguration
    {
        public string Name { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;
    }
}
