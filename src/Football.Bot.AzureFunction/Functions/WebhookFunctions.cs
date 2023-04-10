using System;
using System.Threading.Tasks;
using Football.Bot.Models;
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

    public WebhookFunctions(TelegramBotClient client, HostInfo hostInfo)
    {
        _client = client;
        _hostInfo = hostInfo;
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

        await _client.SetWebhookAsync(uri);

        return new OkObjectResult(new {status = "ok"});
    }
}