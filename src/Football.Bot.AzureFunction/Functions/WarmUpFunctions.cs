using System.Net.Http;
using System.Threading.Tasks;
using Football.Bot.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public class WarmUpFunctions
{
    private readonly HttpClient httpClient;
    private readonly WarmupConfiguration configuration;

    public WarmUpFunctions(HttpClient  httpClient, WarmupConfiguration configuration)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
    }

    [FunctionName("warmup")]
    public async Task Warmup(
        [TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo,
        ILogger log)
    {
        var functionUrl = configuration.Url;

        log.LogInformation("Warmup trigger function processed a request");

        var request = new HttpRequestMessage(HttpMethod.Get, functionUrl);
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            log.LogError("Function warmup failed with status code {statusCode}", response.StatusCode);
        }
    }
}
