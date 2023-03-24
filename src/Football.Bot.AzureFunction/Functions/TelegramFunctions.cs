using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Football.Bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Bot.Functions;

public class TelegramFunctions
{
    private readonly CosmosDbClient _cosmosDbClient;
    private readonly TelegramBotClient _telegramClient;
    
    public TelegramFunctions(CosmosDbClient cosmosDbClient, TelegramBotClient telegramClient)
    {
        _cosmosDbClient = cosmosDbClient;
        _telegramClient = telegramClient;
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

        if (update.Type == UpdateType.Message)
        {
            await HandleUpdate(update);
        }

        return new OkObjectResult("Ok");
    }
    
    private async Task HandleUpdate(Update update)
    {
        var matchInfos = await _cosmosDbClient.Get(Constants.Team);
        var now = DateTime.UtcNow;

        var matches = matchInfos.OrderBy(info => info.Start)
            .Where(matchInfo => matchInfo.Start >= now)
            .Take(3);

        var mathString = DisplayString(matches);
        var message = $"Next 3 matches{Environment.NewLine}{mathString}";

        await _telegramClient.SendTextMessageAsync(update.Message!.Chat, message);
    }

    private static string DisplayString(IEnumerable<MatchInfo> matches)
    {
        var mathString = string.Join("\r\n", matches.Select(info => info.DisplayTitle + ", " + info.Start));
        if (string.IsNullOrWhiteSpace(mathString))
        {
            mathString = "No matches";
        }

        return mathString;
    }
}