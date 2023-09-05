using Azure.Identity;
using AppConfigCoreUtil.Domain;
using AppConfigDriver.Models;

partial class Program
{
    private static string _applicationSettings = "appsettings.json";

    static async Task Main(string[] args)
    {
        bool useService = false;

        if (!useService)
        {
            // Running in stand alone with no pre-loaded objects.

            string endpoint = "https://appconfigbillingtest.azconfig.io";

            string firstLabel = "First";
            string secondLabel = "Second";

            // No need to set KV connection, it just works.You only provide the assembly name
            // when you want to auto load property information for objects used for notifications
            // in a service. Otherwise, just use it as is. 
            AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
                endpoint,
                new DefaultAzureCredential());

            InAssemblyObject? config = await AzureAppConfiguration.LoadSection<InAssemblyObject>(firstLabel);
            if(config != null)
            {
                Console.WriteLine("Configuration should not have been found. Clearing settings.");
                AzureAppConfiguration.DeleteSection<InAssemblyObject>(firstLabel);
            }

            // Set up information
            InAssemblyObject firstObject = new InAssemblyObject()
            {
                Property1 = "Test1",
                Property2 = "Tetst2"
            };
            AzureAppConfiguration.SaveSection(firstObject, firstLabel);

            // Now it's not null
            config = await AzureAppConfiguration.LoadSection<InAssemblyObject>(firstLabel);
            if(config == null) 
            {
                Console.WriteLine("Configuration should have been found. Something went wrong");
            }

            // This one is null
            config = await AzureAppConfiguration.LoadSection<InAssemblyObject>(secondLabel);
            if (config != null)
            {
                Console.WriteLine("Configuration should not have been found. Something went wrong");
            }

            // And then delete it.
            AzureAppConfiguration.DeleteSection<InAssemblyObject>(firstLabel);
        }
        else
        {
            // Run as a service, break in Worker constructor to see the values you set up during 
            // the set up process in the ReadMe.md instructions.
            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configureConfigurations)
            .ConfigureServices(configureServices)
            .Build();

            await host.RunAsync();
        }
    }
}

