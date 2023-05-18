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
    private readonly CommandHandler commandHandler;
    private readonly TelegramConfiguration telegramConfiguration;

    public TelegramFunctions(TelegramConfiguration telegramConfiguration, CommandHandler commandHandler)
    {
        this.commandHandler = commandHandler;
        this.telegramConfiguration = telegramConfiguration;
    }

    [FunctionName("webhook")]
    public async Task<IActionResult> Webhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [ServiceBus("pl-queue", Connection = "ServiceBusConnection")] IAsyncCollector<Update> output,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        if (!ValidateSecret(req))
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
        int deliveryCount,
        DateTime enqueuedTimeUtc,
        string messageId,
        ILogger log)
    {
        log.LogInformation(
            "C# ServiceBus queue trigger function processed message: {@myQueueItem}, {enqueuedTimeUtc}, {deliveryCount}, {messageId}",
            update, enqueuedTimeUtc, deliveryCount, messageId);

        await commandHandler.Execute(update.Message!);
    }

    private bool ValidateSecret(HttpRequest req) =>
        HeadersValidator.ContainsSecret(req.Headers, telegramConfiguration.Secret);
}

public static class HeadersValidator
{
    public static bool ContainsSecret(IHeaderDictionary headers, string secret) =>
        headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var header) &&
        header.Contains(secret);
}
