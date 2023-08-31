namespace AppConfigCoreUtil.Models
{
    internal class ConfigurationIdentity
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ContentType { get;set ; } = string.Empty;
        public string UnderlyingProperty { get; set; } = string.Empty;
    }

    internal class ConfigurtionIdentities
    {
        public bool SectionAsProperty { get; set; } = false;
        public List<ConfigurationIdentity> IdentityList { get; set; } = new List<ConfigurationIdentity>();
    }
}
