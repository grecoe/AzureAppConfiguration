namespace AppConfigDriver.Models
{
    using AppConfigCoreUtil.Attributes;

    /// <summary>
    /// When not using a service, create objects for AppConfig in the same assembly. 
    /// </summary>
    [ConfigurationSection("InAssembly")]
    internal class InAssemblyObject
    {
        [PropertyConfiguration("InAssembly:Property1",
            contentType: AppConfigurationContentType.CONTENT_STRING,
            notify: true)]
        public string Property1 { get; set; } = string.Empty;

        [PropertyConfiguration("InAssembly:Property2",
            contentType: AppConfigurationContentType.CONTENT_STRING,
            notify: true)]
        public string Property2 { get; set; } = string.Empty;
    }
}
