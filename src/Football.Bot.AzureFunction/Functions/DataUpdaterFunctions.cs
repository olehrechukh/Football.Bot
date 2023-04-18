using System.Threading.Tasks;
using Football.Bot.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public class DataUpdaterFunctions
{
    private readonly CosmosDbClient cosmosDbClient;
    private readonly SchedulerProvider schedulerProvider;

    public DataUpdaterFunctions(CosmosDbClient cosmosDbClient, SchedulerProvider schedulerProvider)
    {
        this.cosmosDbClient = cosmosDbClient;
        this.schedulerProvider = schedulerProvider;
    }

    [FunctionName("scheduleUpdate")]
    public async Task RunAsync([TimerTrigger("0 * * * *", RunOnStartup = true)] TimerInfo myTimer,
        ILogger log)
    {
        log.LogInformation("TimeUpdate trigger function processed a request");

        var matches = await schedulerProvider.GetNextMatches(3);

        await cosmosDbClient.Add(matches, Constants.Team);
    }
}
