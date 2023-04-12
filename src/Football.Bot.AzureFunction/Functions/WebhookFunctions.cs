using System;
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
    private readonly TelegramBotClient _client;
    private readonly HostInfo _hostInfo;
    private readonly TelegramConfiguration _telegramConfiguration;

    public WebhookFunctions(TelegramBotClient client, HostInfo hostInfo, TelegramConfiguration telegramConfiguration)
    {
        _client = client;
        _hostInfo = hostInfo;
        _telegramConfiguration = telegramConfiguration;
    }

    [FunctionName("setWebhook")]
    public async Task<IActionResult> HttpTime(
        [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)]
        HttpRequest req, ILogger log)
    {
        var webhookInfo = await _client.GetWebhookInfoAsync();

        if (!string.IsNullOrWhiteSpace(webhookInfo.Url))
        {
            log.LogInformation("Delete webhook {url}", webhookInfo.Url);
            await _client.DeleteWebhookAsync();
        }
        
        var uri = new Uri(new Uri(_hostInfo.Url), "/api/webhook").ToString();
        
        log.LogInformation("Set webhook {url}", uri);

        
        // TODO: Replace with native implementation after it has been updated in v19.
        await _client.SetWebhookWithTokenAsync(uri, secretToken: _telegramConfiguration.Secret);

        return new OkObjectResult(new {status = "ok"});
    }
}