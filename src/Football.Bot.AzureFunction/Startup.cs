using System.Net.Http;
using Football.Bot;
using Football.Bot.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Football.Bot;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = BuildConfiguration(builder.GetContext().ApplicationRootPath);
        builder.Services.AddSingleton(configuration);

        builder.Services.AddHttpClient();
        builder.Services.AddTransient<SchedulerProvider>();
        builder.Services.AddSingleton<CosmosDbClient>();
        builder.Services.AddTransient<CosmosClient>();

        builder.Services.AddSingleton(_ =>
        {
            var cosmos = configuration.GetSection("Cosmos").Get<CosmosConfiguration>();

            var client = new CosmosClient(accountEndpoint: cosmos.Endpoint, authKeyOrResourceToken: cosmos.Token);
            var properties = new ContainerProperties(id: cosmos.Container, partitionKeyPath: "/categoryId");

            var response = client.CreateDatabaseIfNotExistsAsync(cosmos.Database).GetAwaiter().GetResult();
            var container = response.Database.CreateContainerIfNotExistsAsync(properties).GetAwaiter().GetResult();

            return container.Container;
        });

        builder.Services.AddTransient(serviceProvider =>
        {
            var token = configuration.GetValue<string>("TelegramToken");
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();

            var telegramClient = new TelegramBotClient(token, httpClient);

            return telegramClient;
        });
    }

    private static IConfiguration BuildConfiguration(string applicationRootPath)
    {
        // var envConfiguration = GetEnvConfigurationRoot();

        // var keyVaultEndpoint = envConfiguration.GetValue<string>("KeyVaultEndpoint");
        // var keyVaultClient = GetKeyVaultClient();
        
        
        var config = new ConfigurationBuilder()
            .SetBasePath(applicationRootPath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Startup>()
            // .AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager())
            .AddEnvironmentVariables()
            .Build();

        return config;
    }

    private static IConfigurationRoot GetEnvConfigurationRoot() => new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

    private static KeyVaultClient GetKeyVaultClient()
    {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        // Create a new Key Vault client with Managed Identity authentication
        var callback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
        var keyVaultClient = new KeyVaultClient(callback);
        return keyVaultClient;
    }
}

public class CosmosConfiguration
{
    public string Token { get; set; }
    public string Endpoint { get; set; }
    public string Database { get; set; }
    public string Container { get; set; }
}