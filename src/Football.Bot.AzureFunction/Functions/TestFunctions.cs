using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public class TestFunctions
{
    [FunctionName("test")]
    public IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        return new OkObjectResult(new {status = "ok", v = 1.2});
    }
}