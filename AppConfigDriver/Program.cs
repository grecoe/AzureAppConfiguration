using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using RPAppConfig.Models;
using RPAppConfig.Workers;
using AppConfigCoreUtil.Domain;
using AppConfigCoreUtil.Models;
using AppConfigModelLib.Models.Sections;

class Program
{
    private static string _applicationSettings = "appsettings.json";
    private static IConfigurationRefresher? _refresher = null;
    private static ConfigurationProperties? ConfigurationProperties = null;

    static async Task Main(string[] args)
    {
        //Assembly.GetEntryAssembly().GetReferencedAssemblies();

        /*
        string endpoint = "https://appconfigbillingtest.azconfig.io";
        AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
            endpoint,
            new DefaultAzureCredential());

        // Before loading anything else from it, you can access data if you have a configured
        // class meaning you do NOT need to have a model library.
        var xx = await AzureAppConfiguration.LoadSection<SoloSection>("Development");
        */

        /* Test out functonality - already built into webapp/worker 
        string endpoint = "https://appconfigbillingtest.azconfig.io";
        AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
            endpoint,
            new DefaultAzureCredential());

        AzureAppConfiguration.DeleteSection<SoloSection>(AppConfigurationEnvironmentLabel.ENV_PRODUCTION);

        SoloSection? ss = await AzureAppConfiguration.LoadSection<SoloSection>(
            AppConfigurationEnvironmentLabel.ENV_DEVELOPMENT);

        ss.SoloName = "Production";
        //AzureAppConfiguration.SaveSection(ss, AppConfigurationEnvironmentLabel.ENV_PRODUCTION);

        TestSection? ts = await AzureAppConfiguration.LoadSection<TestSection>(
            AppConfigurationEnvironmentLabel.ENV_DEVELOPMENT);


        AzureAppConfiguration.DeleteSection<TestSection>("Production");
        AzureAppConfiguration.SaveSection(tp, "Production");

        TestSection? tp2 = await AzureAppConfiguration.LoadSection<TestSection>(
            AppConfigurationEnvironmentLabel.ENV_PRODUCTION);

        var maps = AzureAppConfiguration.GetConfigurationMapping();

        var setting = AzureAppConfiguration.ConfigurationClient.GetConfigurationSetting(
            "Common:Cosmos",
            AppConfigurationEnvironmentLabel.ENV_DEVELOPMENT);

        AzureAppConfiguration.SaveSection(tp, "Production");
        */

        IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(configureConfigurations)
        .ConfigureServices(configureServices)
        .Build();

        await host.RunAsync();
    }

    /// <summary>
    /// Set up the configurations to be passed to the service. Should be BOTH the appsettings.json, which
    /// is also required to get work done, and the azure app configuration service.
    /// </summary>
    static void configureConfigurations(HostBuilderContext context, IConfigurationBuilder config)
    {
        TokenCredential credential = new DefaultAzureCredential();
        ConfigurationProperties? configurationProperties = GetConfigurationProperties(context, _applicationSettings);

        if(configurationProperties == null)
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
        AzureAppConfiguration AzureAppConfiguration = new AzureAppConfiguration(
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
            options.Connect(new Uri(AzureAppConfiguration.Endpoint), credential)
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credential);
            });

            // You do NOT need to know all of the configurations or what to target for 
            // a notification update, the AzureAppConfiguration has all that information
            // from objects loaded in ModelLibrary and likely you'll want to target all of them 
            // for reading whether you read them or not.
            ConfigurationMapping mapping = AzureAppConfiguration.GetConfigurationMapping();
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
    static void configureServices(HostBuilderContext context, IServiceCollection services)
    {
        // You DO HAVE to know the section name AND class that supports it here so that 
        // they are available in your worker service. 
        services.AddOptions<CosmosConfiguration>().Bind(context.Configuration.GetSection("Cosmos"));
        services.AddOptions<SinglePropConfiguration>().Bind(context.Configuration.GetSection("SingleProp:Data"));

        // Add logging, refresher and worker service
        services.AddLogging();
        services.AddSingleton(_refresher);
        services.AddHostedService<Worker>();
    }

    /// <summary>
    /// Used to load the appsettings.json file as it has the AppConfiguration endpoint and other
    /// configuration values needed.
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

