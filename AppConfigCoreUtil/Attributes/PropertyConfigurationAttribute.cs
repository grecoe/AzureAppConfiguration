namespace AppConfigCoreUtil.Attributes
{
    /// <summary>
    /// Definitions of the content types supported by AppConfiguration.
    /// </summary>
    public class AppConfigurationContentType
    {
        public const string CONTENT_STRING = "string";
        public const string CONTENT_JSON = "application/json";
        public const string CONTENT_KV = "application/vnd.microsoft.appconfig.keyvaultref";
    }

    /// <summary>
    /// Example hard coded types of environment labels. 
    /// </summary>
    public class AppConfigurationEnvironmentLabel
    {
        public const string ENV_PRODUCTION = "Production";
        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_CANARY = "Canary";
        public const string ENV_STAGING = "Staging";
    }

    /// <summary>
    /// An attribute for specific properties that map to a key/label pair 
    /// in app configuration.
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
        /// Where 'property' matches the name of the class field being mapped.
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// The AppConfiguration content type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// A flag on whether this property should create a notification
        /// when changed. 
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
