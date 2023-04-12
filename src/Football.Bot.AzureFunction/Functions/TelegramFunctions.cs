using System;
using System.Linq;
using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Football.Bot.Models;
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
    private readonly TelegramConfiguration _telegramConfiguration;

    public TelegramFunctions(CommandHandler commandHandler, TelegramConfiguration telegramConfiguration)
    {
        _commandHandler = commandHandler;
        _telegramConfiguration = telegramConfiguration;
    }

    [FunctionName("webhook")]
    public async Task<IActionResult> Webhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [ServiceBus("pl-queue", Connection = "ServiceBusConnection")]
        IAsyncCollector<Update> output,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        if (!MatchSecretValue(req))
        {
            return new StatusCodeResult(401);
        }

        var requestBody = await req.ReadAsStringAsync();
        var update = JsonConvert.DeserializeObject<Update>(requestBody);

        if (update is null)
        {
            return new BadRequestResult();
        }

        if (update is {Type: UpdateType.Message, Message.Type: MessageType.Text})
        {
            await output.AddAsync(update);
        }

        return new OkObjectResult("Ok");
    }


    [FunctionName("newMessageQueue")]
    public async Task Run(
        [ServiceBusTrigger("pl-queue", Connection = "ServiceBusConnection")]
        Update update,
        Int32 deliveryCount,
        DateTime enqueuedTimeUtc,
        string messageId,
        ILogger log)
    {
        log.LogInformation(
            "C# ServiceBus queue trigger function processed message: {@myQueueItem}, {enqueuedTimeUtc}, {deliveryCount}, {messageId}",
            update, enqueuedTimeUtc, deliveryCount, messageId);

        await _commandHandler.Execute(update.Message!);
    }

    private bool MatchSecretValue(HttpRequest req) =>
        HeadersValidator.MatchSecretValue(req.Headers, _telegramConfiguration.Secret);
}

public static class HeadersValidator
{
    public static bool MatchSecretValue(IHeaderDictionary headers, string secret) =>
        headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var header) &&
        header.Contains(secret);
}