namespace AppConfigCoreUtil.Attributes
{
    public class AppConfigurationContentType
    {
        public const string CONTENT_STRING = "string";
        public const string CONTENT_JSON = "application/json";
        public const string CONTENT_KV = "application/vnd.microsoft.appconfig.keyvaultref";
    }

    public class AppConfigurationEnvironmentLabel
    {
        public const string ENV_PRODUCTION = "Production";
        public const string ENV_DEVELOPMENT = "Development";
        public const string ENV_CANARY = "Canary";
        public const string ENV_STAGING = "Staging";
    }

    [AttributeUsage(AttributeTargets.Property |
                    AttributeTargets.Class,
                    AllowMultiple = false)
    ]
    public class PropertyConfigurationAttribute : Attribute
    {
        public string Key { get; set; }
        public bool Notify { get; set; }
        public string ContentType { get; set; }

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
