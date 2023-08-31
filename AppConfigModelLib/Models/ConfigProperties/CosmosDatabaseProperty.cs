namespace AppConfigModelLib.Models.Properties
{
    /// <summary>
    /// An embedded object from the ConsosConfiguration section.
    /// </summary>
    public class CosmosDatabaseProperty
    {
        public string Database { get; set; } = string.Empty;
        public List<string> Collections { get; set; } = new List<string>();
    }
}
