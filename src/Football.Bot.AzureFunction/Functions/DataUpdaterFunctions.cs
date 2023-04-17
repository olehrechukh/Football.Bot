using System.Threading.Tasks;
using Football.Bot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public class DataUpdaterFunctions
{
    private readonly CosmosDbClient _cosmosDbClient;
    private readonly SchedulerProvider _schedulerProvider;

    public DataUpdaterFunctions(CosmosDbClient cosmosDbClient, SchedulerProvider schedulerProvider)
    {
        _cosmosDbClient = cosmosDbClient;
        _schedulerProvider = schedulerProvider;
    }

    [FunctionName("scheduleUpdate")]
    public async Task RunAsync([TimerTrigger("0 * * * *", RunOnStartup = true)] TimerInfo myTimer,
        ILogger log)
    {
        log.LogInformation("TimeUpdate trigger function processed a request");

        var matches = await _schedulerProvider.GetNextMatches(3);

        await _cosmosDbClient.Add(matches, Constants.Team);
    }
}