using AppConfigCoreUtil.Domain;
using AppConfigModelLib.Models.Sections;
using AppConfigDriver.Models;
using AppConfigDriver.Workers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

partial class Program
{
    private static string _applicationSettings = "appsettings.json";
    private static AzureAppConfiguration? AppConfiguration = null;
    private static ConfigurationProperties? ConfigurationProperties = null;


    /// <summary>
    /// Set up the configurations to be passed to the service. Should be BOTH the appsettings.json, which
    /// is also required to get work done, and the azure app configuration service.
    /// </summary>
    static void ConfigureConfigurations(HostBuilderContext context, IConfigurationBuilder config)
    {
        TokenCredential credential = new DefaultAzureCredential();
        ConfigurationProperties? configurationProperties = GetConfigurationProperties(context, _applicationSettings);

        if (configurationProperties == null)
        {
            throw new Exception("Cannot get appsettings.json data");
        }

        // Use the active label in your appsetting.json and you can easily toggle between Prod,
        // Development, Test settings on the fly.
        string activeLabel = string.IsNullOrEmpty(configurationProperties.ActiveSettingsLabel) ?
            LabelFilter.Null :
            configurationProperties.ActiveSettingsLabel;

        // Create the app configuration class wrapper that manages the CRUD operations
        // to the AppConfiguration class. The ModelLibrary name is passed so that you can
        // have your models of data from AppConfiguration stored in a separate class library. 
        //
        // If you store them in the same assembly as your application, pass your app name in.
        AppConfiguration = new AzureAppConfiguration(
            configurationProperties.AzureAppConfigurationEndpoint,
            credential,
            configurationProperties.ModelLibrary);

        // Configure the AppConfiguration with notifications and the active label
        AppConfiguration.ConfigureAppConfiguration(config, activeLabel);
    }


    /// <summary>
    /// Set up the main dependencies to inject, most importantly is going to be the options monitors
    /// for the azure app configuration service. 
    /// </summary>
    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddLogging();
        services.AddHostedService<Worker>();

        // If we have teh app configuration AND there is a provider for it, then we can
        // set up the options monitors for the sections we want to monitor and add a refresher
        if (AppConfiguration != null)
        {
            ConfigurationRoot? configRoot = (context.Configuration as ConfigurationRoot);
            if(configRoot != null )
            {
                if (configRoot.Providers.Select(x => x.GetType().Name).Contains("AzureAppConfigurationProvider"))
                {
                    // You DO HAVE to know the section name AND class that supports it here so that 
                    // they are available in your worker service. 
                    services.AddOptions<CosmosConfiguration>().Bind(context.Configuration.GetSection("Cosmos"));
                    services.AddOptions<SinglePropConfiguration>().Bind(context.Configuration.GetSection("SingleProp:Data"));

                    // Add refresher if present
                    if (AppConfiguration.Refresher != null)
                    {
                        services.AddSingleton(AppConfiguration.Refresher);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Used to load the appsettings.json file as it has the AppConfiguration endpoint and other
    /// configuration values needed.
    /// 
    /// Just as easily could be environment variables.
    /// </summary>
    static ConfigurationProperties? GetConfigurationProperties(HostBuilderContext context, string appSettingsFile)
    {
        if (Program.ConfigurationProperties == null)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(context.HostingEnvironment.ContentRootPath)
                .AddJsonFile(appSettingsFile, optional: false)
                .Build();

            Program.ConfigurationProperties = configuration.GetSection("ConfigurationProperties").Get<ConfigurationProperties>();
        }
        return Program.ConfigurationProperties;
    }
}
