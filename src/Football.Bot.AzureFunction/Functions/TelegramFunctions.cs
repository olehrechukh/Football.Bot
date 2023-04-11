using System;
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
    public async Task<IActionResult> Webhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
        HttpRequest req,
        [ServiceBus("pl-queue", Connection = "ServiceBusConnection")]
        IAsyncCollector<Update> output,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request");

        var requestBody = await req.ReadAsStringAsync();
        var update = JsonConvert.DeserializeObject<Update>(requestBody);

        if (update is null)
        {
            return new BadRequestResult();
        }

        if (update.Type == UpdateType.Message && update.Message?.Type == MessageType.Text)
        {
            await output.AddAsync(update);
        }

        return new OkObjectResult("Ok");
    }

    [FunctionName("newMessageQueue")]
    public async Task Run(
        [ServiceBusTrigger("pl-queue", Connection = "ServiceBusConnection")]
        Update myQueueItem,
        Int32 deliveryCount,
        DateTime enqueuedTimeUtc,
        string messageId,
        ILogger log)
    {
        log.LogInformation(
            "ServiceBus queue trigger function processed message: {@myQueueItem}, {enqueuedTimeUtc}, {deliveryCount}, {messageId}",
            myQueueItem, enqueuedTimeUtc, deliveryCount, messageId);

        await _commandHandler.Execute(myQueueItem.Message);
    }
}