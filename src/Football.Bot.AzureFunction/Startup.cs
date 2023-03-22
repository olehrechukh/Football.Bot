using System.Net.Http;
using Football.Bot;
using Football.Bot.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
            var container = client.GetDatabase(cosmos.Database).GetContainer(cosmos.Container);

            return container;
        });

        builder.Services.AddTransient(serviceProvider =>
        {
            var token = configuration.GetValue<string>("TelegramToken");
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();

            var telegramClient = new TelegramBotClient(token, httpClient);

            return telegramClient;
        });
    }

    private IConfiguration BuildConfiguration(string applicationRootPath)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(applicationRootPath)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Startup>()
            .AddEnvironmentVariables()
            .Build();

        return config;
    }
}

public class CosmosConfiguration
{
    public string Token { get; set; }
    public string Endpoint { get; set; }
    public string Database { get; set; }
    public string Container { get; set; }
}