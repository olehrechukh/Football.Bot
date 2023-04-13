using System;
using Microsoft.AspNetCore.Http;

namespace Football.Bot.Services;

public static class TelegramUriHelper
{
    public static string ConvertToTelegramWebhook(string scheme, HostString hostString) =>
        ExtractUri(scheme, hostString)
            .ConvertToTelegramWebhook();

    private static UriBuilder ExtractUri(string scheme, HostString hostString)
    {
        var uriBuilder = new UriBuilder(scheme, hostString.Host);

        if (hostString.Port.HasValue)
        {
            uriBuilder.Port = hostString.Port.Value;
        }

        return uriBuilder;
    }

    private static string ConvertToTelegramWebhook(this UriBuilder uriBuilder)
    {
        uriBuilder.Path = "/api/webhook";

        var uri = uriBuilder.ToString();

        return uri;
    }
}