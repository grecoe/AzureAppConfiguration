using AppConfigCoreUtil.Domain;
using AppConfigCoreUtil.Models;
using AppConfigModelLib.Models.Sections;
using AppConfigDriver.Models;
using AppConfigDriver.Workers;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

partial class Program
{
    private static string _applicationSettings = "appsettings.json";
    private static IConfigurationRefresher? _refresher = null;
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
        AzureAppConfiguration appConfiguration = new AzureAppConfiguration(
            configurationProperties.AzureAppConfigurationEndpoint,
            credential,
            configurationProperties.ModelLibrary);

        config.AddAzureAppConfiguration(options =>
        {
            // REQUIREMENTS - Same identity is used in AppConfig and KeyVault
            // AppConfiguration = "App Configuration Data Owner"
            // KeyVault = "Key Vault Secrets Officer"

            // Because we are using values from a keyvault we need to also make the connection
            // to that vault. Ensure you have provided yourself with the proper rights.
            options.Connect(new Uri(appConfiguration.Endpoint), credential)
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credential);
            });

            // You do NOT need to know all of the configurations or what to target for 
            // a notification update, the AzureAppConfiguration has all that information
            // from objects loaded in ModelLibrary and likely you'll want to target all of them 
            // for reading whether you read them or not.
            ConfigurationMapping mapping = appConfiguration.GetConfigurationMapping();
            foreach (KeyValuePair<string, SectionConfiguration> map in mapping.SectionMappings)
            {
                options.Select(string.Format("{0}*", map.Key), activeLabel);
            }

            // Similarly you don't need to know the fields to listen on, just add them all.
            // If in fact you don't want to listen to them all, you can filter out which ones
            // you want here. 
            List<string> changeNotificationFields = mapping.SectionMappings
                .Select(x => x.Value.NotificationFields)
                .SelectMany(field => field)
                .ToList();

            options.ConfigureRefresh(refresh =>
            {
                foreach (string field in changeNotificationFields)
                {
                    refresh
                        .Register(field, activeLabel, false)
                        .SetCacheExpiration(TimeSpan.FromSeconds(5));
                }
            });

            _refresher = options.GetRefresher();
        });
    }


    /// <summary>
    /// Set up the main dependencies to inject, most importantly is going to be the options monitors
    /// for the azure app configuration service. 
    /// </summary>
    static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // You DO HAVE to know the section name AND class that supports it here so that 
        // they are available in your worker service. 
        services.AddOptions<CosmosConfiguration>().Bind(context.Configuration.GetSection("Cosmos"));
        services.AddOptions<SinglePropConfiguration>().Bind(context.Configuration.GetSection("SingleProp:Data"));

        // Add logging, refresher and worker service
        services.AddLogging();
        if(_refresher != null)
        {
            services.AddSingleton(_refresher);
        }
        services.AddHostedService<Worker>();
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
