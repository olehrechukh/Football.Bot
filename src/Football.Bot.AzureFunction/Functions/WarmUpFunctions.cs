using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public static class WarmUpFunctions
{
    [FunctionName("warmup")]
    public static async Task Warmup(
        [TimerTrigger("0 */1 * * * *")] TimerInfo timerInfo,
        HttpClient client,
        ExecutionContext context,
        ILogger log)
    {
        var functionUrl = $"{context.FunctionAppDirectory}/";

        var request = new HttpRequestMessage(HttpMethod.Get, functionUrl);
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            log.LogError("Function warmup failed with status code {statusCode}", response.StatusCode);
        }
    }

    [FunctionName("warmup1")]
    public static async Task<ActionResult> Warmup1(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpClient client,
        ExecutionContext context,
        ILogger log)
    {
        var functionUrl = $"{context.FunctionAppDirectory}/";

        return new OkObjectResult(functionUrl);
    }
}
