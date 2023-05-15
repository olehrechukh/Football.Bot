using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Football.Bot.Functions;

public static class TestFunctions
{
    [FunctionName("test")]
    public static IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        return new OkObjectResult(new TestResponse(Status: "ok", V: 1.2));
    }
}

public record TestResponse(string Status, double V);
