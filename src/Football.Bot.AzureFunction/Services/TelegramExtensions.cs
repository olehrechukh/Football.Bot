using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Football.Bot.Services;

public static class TelegramExtensions
{
    // NOTE: Telegram.Bot doesnt support secret_token in v18
    // It will implemented in v19 https://github.com/TelegramBots/Telegram.Bot/issues/1109
    // TODO: Delete after it has been fixed in v19.
    public static async Task SetWebhookWithTokenAsync(
        this ITelegramBotClient botClient,
        string url,
        InputFileStream? certificate = default,
        string? ipAddress = default,
        int? maxConnections = default,
        IEnumerable<UpdateType>? allowedUpdates = default,
        bool? dropPendingUpdates = default,
        string? secretToken = default,
        CancellationToken cancellationToken = default
    )
    {
        await botClient.MakeRequestAsync(
                request: new SetWebhookRequestWithToken(url)
                {
                    Certificate = certificate,
                    IpAddress = ipAddress,
                    MaxConnections = maxConnections,
                    AllowedUpdates = allowedUpdates,
                    DropPendingUpdates = dropPendingUpdates,
                    SecretToken = secretToken
                },
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public class SetWebhookRequestWithToken : SetWebhookRequest
    {
        /// <summary>
        /// Upload your public key certificate so that the root certificate in use can be checked.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? SecretToken { get; set; }

        public SetWebhookRequestWithToken(string url) : base(url)
        {
        }
    }
}