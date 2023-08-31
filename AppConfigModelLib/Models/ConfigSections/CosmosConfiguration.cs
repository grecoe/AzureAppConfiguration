namespace AppConfigModelLib.Models.Sections
{
    using AppConfigCoreUtil.Attributes;
    using AppConfigModelLib.Models.Properties;

    /// <summary>
    /// Mock data required for a Cosmos DB configuration settings standpoint. 
    /// </summary>
    [ConfigurationSection("Cosmos")]
    public class CosmosConfiguration
    {
        /// <summary>
        /// Property starts with the section name and ends with the property name. 
        /// Property name must match that in the AzureAppConfiguration along with the key
        /// </summary>
        [PropertyConfiguration("Cosmos:Enabled", notify: true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Property starts with the section name and ends with the property name. 
        /// Property name must match that in the AzureAppConfiguration along with the key
        /// 
        /// KeyVault values are NOT updated in the AppConfiguration, these require a manual 
        /// relead if you think it changed. 
        /// </summary>
        [PropertyConfiguration(
            "Cosmos:ConnectionString",
            contentType: AppConfigurationContentType.CONTENT_KV,
            notify: false)]
        public string ConnectionString { get; set; } = string.Empty;


        /// <summary>
        /// Property starts with the section name and ends with the property name. 
        /// Property name must match that in the AzureAppConfiguration along with the key
        /// 
        /// For embedded objects you MUST use application/jdon in the configuration here as 
        /// well as it's type in the AppConfiguration itself. 
        /// </summary>
        [PropertyConfiguration(
            "Cosmos:Database", 
            contentType: AppConfigurationContentType.CONTENT_JSON,
            notify:true)]
        public CosmosDatabaseProperty Database{ get; set; } = new CosmosDatabaseProperty();
    }
}
