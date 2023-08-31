namespace RPAppConfig.Workers
{
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.Options;
    using AppConfigModelLib.Models.Sections;

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        private readonly IOptionsMonitor<CosmosConfiguration> _cosmosConfiguration;
        private readonly IOptionsMonitor<SinglePropConfiguration> _singlePropConfiguration;
        private readonly IConfigurationRefresher _refresher;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration configuration,
            // Have to know WHAT you are trying to monitor
            IOptionsMonitor<CosmosConfiguration> cosmosConfig,
            IOptionsMonitor<SinglePropConfiguration> singlePropConfig,
            IConfigurationRefresher refresher)
        {
            _logger = logger;
            _configuration = configuration;

            _cosmosConfiguration = cosmosConfig;
            _singlePropConfiguration = singlePropConfig;
            _refresher = refresher;

            _cosmosConfiguration.OnChange(OnSettingsChange);
            _singlePropConfiguration.OnChange(OnSettingsChange);

            // FYI> You also have access to the appsettings.json file here
            //var configprops = configuration.GetSection("ConfigurationProperties").Get<ConfigurationProperties>();
        }

        private void OnSettingsChange<T>(T settings)
            where T : class, new()
        {
            _logger.LogInformation($"{typeof(T).Name} changed");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int count = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"{count} :: {_cosmosConfiguration.CurrentValue.Enabled}");
                count++;

                // Force an update on the settings, if any OnSettingsChange is triggered.
                await _refresher.TryRefreshAsync(stoppingToken);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}