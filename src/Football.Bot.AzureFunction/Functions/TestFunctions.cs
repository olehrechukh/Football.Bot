using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Football.Bot.Functions;

public class TestFunctions
{
    [FunctionName("test")]
    public IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        return new OkObjectResult(new {status = "ok", v = 1.2});
    }
}