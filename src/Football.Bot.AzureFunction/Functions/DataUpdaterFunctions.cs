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

    [FunctionName("TimeUpdate")]
    public async Task RunAsync([TimerTrigger("*/5 * * * * *"
#if DEBUG
            , RunOnStartup = true
#endif
        )]
        TimerInfo myTimer, ILogger log)
    {
        var matches = await _schedulerProvider.GetNextMatches();

        await _cosmosDbClient.Add(matches, Constants.Team, log);
    }
}