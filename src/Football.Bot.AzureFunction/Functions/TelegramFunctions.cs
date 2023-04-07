using System.IO;
using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Bot.Functions;

public class TelegramFunctions
{
    private readonly CommandHandler _commandHandler;

    public TelegramFunctions(CommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    [FunctionName("webhook")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var update = JsonConvert.DeserializeObject<Update>(requestBody);

        if (update is null)
        {
            return new BadRequestResult();
        }

        if (update.Type == UpdateType.Message && update.Message?.Type == MessageType.Text)
        {
            await HandleUpdate(update.Message);
        }

        return new OkObjectResult("Ok");
    }

    private async Task HandleUpdate(Message message)
    {
        await _commandHandler.Execute(message);
    }
}