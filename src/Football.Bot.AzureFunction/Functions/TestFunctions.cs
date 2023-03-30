using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Football.Bot.Functions;

public class TestFunctions1
{
    [FunctionName("test1")]
    public IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        return new OkObjectResult(new {status = "ok", v = 1.2});
    }
}

public class TestFunctions
{
    private readonly IConfiguration _configuration;

    public TestFunctions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [FunctionName("test")]
    public IActionResult Test(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        return new OkObjectResult(new {status = "ok", v = 1.2});
    }

    [FunctionName("variable")]
    public IActionResult TestVariable(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
        HttpRequest req, ILogger log)
    {
        var value = _configuration.GetValue<string>("test-variable");
        return new OkObjectResult(new {status = "ok", test_variable = value});
    }
}