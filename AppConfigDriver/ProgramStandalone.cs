using Azure.Identity;
using AppConfigCoreUtil.Domain;
using AppConfigDriver.Models;
using AppConfigModelLib.Models.Sections;
using AppConfigCoreUtil.Attributes;

partial class Program
{
    private static string AppConfigEndponit = "https://appconfigbillingtest.azconfig.io";

    static async Task ExecuteIndividualProperties()
    {
        // No need to set KV connection, it just works.You only provide the assembly name
        // when you want to auto load property information for objects used for notifications
        // in a service. Otherwise, just use it as is. 
        AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
            Program.AppConfigEndponit,
            new DefaultAzureCredential());

        // Single prop information
        string propKey = "Dummy:Prop";
        string? propLabel = null;
        string propContentType = AppConfigurationContentType.CONTENT_STRING;
        string propValue = "Test";
        string propValue2 = "Test2";

        // Create
        Console.WriteLine("Create single property");
        AzureAppConfiguration.CreateOrUpdateConfigurationSetting(propKey, propValue, propContentType, propLabel);

        // Read
        Console.WriteLine("Read single property");
        string? storedValue = await AzureAppConfiguration.GetConfigurationSetting<string?>(propKey, propLabel);
        if(storedValue == null || storedValue != propValue)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        // Update
        Console.WriteLine("Update single property");
        AzureAppConfiguration.CreateOrUpdateConfigurationSetting(propKey, propValue2, propContentType, propLabel);
        storedValue = await AzureAppConfiguration.GetConfigurationSetting<string?>(propKey, propLabel);
        if (storedValue == null || storedValue != propValue2)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        // Delete
        Console.WriteLine("Delete single property");
        AzureAppConfiguration.DeleteConfigurationSetting(propKey, propLabel);
        storedValue = await AzureAppConfiguration.GetConfigurationSetting<string?>(propKey, propLabel);
        if (storedValue != null)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        Console.WriteLine("Test done with single prop");
    }


    static async Task ExecuteObjectProperties()
    {
        // No need to set KV connection, it just works.You only provide the assembly name
        // when you want to auto load property information for objects used for notifications
        // in a service. Otherwise, just use it as is. 
        AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
            Program.AppConfigEndponit,
            new DefaultAzureCredential());

        string[] labels = new string[] { "FirstLabel", "SecondLabel" };

        // Use object from this assembly OR from AppConfigModelLib
        string prop1ValueStart = "Test1";
        string prop1ValueEnd = "Finished";
        InAssemblyObject firstObject = new InAssemblyObject()
        {
            Property1 = prop1ValueStart,
            Property2 = "Tetst2"
        };

        // Create
        Console.WriteLine("Create object properties");
        AzureAppConfiguration.CreateOrUpdateSection<InAssemblyObject>(firstObject, labels[0]);

        // Read
        Console.WriteLine("Read object properties");
        InAssemblyObject? storedObject = await AzureAppConfiguration.GetSection<InAssemblyObject>(labels[0]);
        if (storedObject == null || storedObject.Property1 != prop1ValueStart)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        // Change object then save with new label
        Console.WriteLine("Create second object properties");
        firstObject.Property1 = prop1ValueEnd;
        AzureAppConfiguration.CreateOrUpdateSection<InAssemblyObject>(firstObject, labels[1]);
        // Get both versions
        InAssemblyObject? firstLabel = await AzureAppConfiguration.GetSection<InAssemblyObject>(labels[0]);
        InAssemblyObject? secondLabel = await AzureAppConfiguration.GetSection<InAssemblyObject>(labels[1]);
        if( firstLabel == null || secondLabel == null || firstLabel.Property1 != prop1ValueStart || secondLabel.Property1 != prop1ValueEnd)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        // Delete both 
        Console.WriteLine("Delete both sets of object properties");
        AzureAppConfiguration.DeleteSection<InAssemblyObject>(labels[0]);
        AzureAppConfiguration.DeleteSection<InAssemblyObject>(labels[1]);
        firstLabel = await AzureAppConfiguration.GetSection<InAssemblyObject>(labels[0]);
        secondLabel = await AzureAppConfiguration.GetSection<InAssemblyObject>(labels[1]);
        if (firstLabel != null || secondLabel != null)
        {
            Console.WriteLine("Seems to be an error with the saved value.");
        }

        Console.WriteLine("Test done with objects");

    }

}
