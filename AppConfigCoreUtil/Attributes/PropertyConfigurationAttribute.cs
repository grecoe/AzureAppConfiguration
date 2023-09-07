namespace AppConfigCoreUtil.Attributes
{
    /// <summary>
    /// Constant definitions of the content types supported by AppConfiguration.
    /// 
    /// By default, all values are stored as strings, however there are a couple of 
    /// types that behave differently.. 
    /// 
    /// application/json: Identifies to AppConfiguration that the value is a JSON object
    /// and can be deserialized with the Newtonsoft.Json library, and will automatically be 
    /// deserialized when using it in a service. 
    /// 
    /// application/vnd* : Identifies a value that is coming from an Azure KeyVault. These 
    /// can be read but cannot be written to currently.
    /// </summary>
    public class AppConfigurationContentType
    {
        public const string CONTENT_STRING = "string";
        public const string CONTENT_JSON = "application/json";
        public const string CONTENT_KV = "application/vnd.microsoft.appconfig.keyvaultref";
    }

    /// <summary>
    /// Example hard coded types of environment labels. In code, you can use these types of values
    /// to toggle between which version of a property you are interested in. These have no inherent 
    /// value in the application, but just an example. 
    /// </summary>
    public class AppConfigurationEnvironmentLabel
    {
        public const string ENV_PRODUCTION = "Production";
        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_CANARY = "Canary";
        public const string ENV_STAGING = "Staging";
    }

    /// <summary>
    /// An attribute limited to classes or attributes, used to identify 
    /// an AppConfig property by:
    ///     Key
    ///     ContentType
    ///     Notify (should it notify on change)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property |
                    AttributeTargets.Class,
                    AllowMultiple = false)
    ]
    public class PropertyConfigurationAttribute : Attribute
    {
        /// <summary>
        /// The AppConfiguration key. This is the full key formed as
        /// 
        /// section:property
        /// 
        /// Where 'property' matches the name of the class property being mapped.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The AppConfiguration content type, should be one of AppConfigurationContentType
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Indicates if this proerty, when used in a Service, should be added to the 
        /// change notifications list.
        /// </summary>
        public bool Notify { get; set; }

        public PropertyConfigurationAttribute(
            string key, 
            string contentType = AppConfigurationContentType.CONTENT_STRING, 
            bool notify = true)
        {
            this.Key = !string.IsNullOrEmpty(key) ? key : "*";
            this.Notify = notify;
            this.ContentType = contentType;
        }
    }
}
