using System.Threading.Tasks;
using Football.Bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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
    public async Task RunAsync([TimerTrigger("0 * * * *"
#if DEBUG
            , RunOnStartup = true
#endif
        )]
        TimerInfo myTimer, ILogger log)
    {
        log.LogInformation("TimeUpdate trigger function processed a request");

        var matches = await _schedulerProvider.GetNextMatches();

        await _cosmosDbClient.Add(matches, Constants.Team, log);
    }

    [FunctionName("HttpTimeUpdate")]
    public async Task HttpTime(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("HttpTimeUpdate trigger function processed a request");

        var matches = await _schedulerProvider.GetNextMatches();

        await _cosmosDbClient.Add(matches, Constants.Team, log);
    }
}