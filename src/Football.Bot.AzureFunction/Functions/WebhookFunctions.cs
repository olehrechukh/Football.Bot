using System.Net.Http;
using System.Threading.Tasks;
using Football.Bot.Models;
using Football.Bot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Football.Bot.Functions;

public class WebhookFunctions
{
    private readonly TelegramBotClient client;
    private readonly TelegramConfiguration telegramConfiguration;

    public WebhookFunctions(TelegramBotClient client, TelegramConfiguration telegramConfiguration)
    {
        this.client = client;
        this.telegramConfiguration = telegramConfiguration;
    }

    [FunctionName("setWebhook")]
    public async Task<IActionResult> HttpTime(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        var webhookInfo = await client.GetWebhookInfoAsync();

        if (!string.IsNullOrWhiteSpace(webhookInfo.Url))
        {
            log.LogInformation("Delete webhook {url}", webhookInfo.Url);
            await client.DeleteWebhookAsync();
        }

        var uri = ExtractUri(req);

        log.LogInformation("Set webhook {url}", uri);


        // TODO: Replace with native implementation after it has been updated in v19.
        await client.SetWebhookWithTokenAsync(uri, secretToken: telegramConfiguration.Secret);

        return new OkObjectResult(new {status = "ok"});
    }

    private static string ExtractUri(HttpRequest req) =>
        TelegramUriHelper.ConvertToTelegramWebhook(req.Scheme, req.Host);
}