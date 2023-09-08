partial class Program
{
    static async Task Main(string[] args)
    {
        // By default doesn't use objects as all settings from Readme.md need
        // to be configured. 
        bool useService = false;

        if (!useService)
        {
            // Run outside of a service and access individual properties or
            // properties contained in objects. 
            await ExecuteIndividualProperties();
            await ExecuteObjectProperties();
        }
        else
        {
            // Run as a service, break in Worker constructor to see the values you set up during 
            // the set up process in the ReadMe.md instructions.
            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(ConfigureConfigurations)
            .ConfigureServices(ConfigureServices)
            .Build();

            await host.RunAsync();
        }
    }
}

