using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Football.Bot.Extensions;
using Football.Bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Football.Bot.Commands;

internal class NextMatchCommand : TextBasedCommand
{
    protected override string Pattern => "next";

    private readonly CosmosDbClient _cosmosDbClient;
    private readonly TelegramBotClient _telegramClient;

    public NextMatchCommand(CosmosDbClient cosmosDbClient, TelegramBotClient telegramClient)
    {
        _cosmosDbClient = cosmosDbClient;
        _telegramClient = telegramClient;
    }

    public override async Task Execute(Message message)
    {
        var now = DateTime.UtcNow;
        var matchInfos = await _cosmosDbClient.Get(Constants.Team);

        var matches = matchInfos
            .OrderBy(info => info.Start)
            .Where(matchInfo => matchInfo.Start >= now)
            .Take(3);

        var displayString = DisplayString(matches);

        await _telegramClient.SendTextMessageAsync(message.Chat, displayString);
    }

    private static string DisplayString(IEnumerable<MatchInfo> matches)
    {
        var mathString = string.Join("\r\n",
            matches.Select(info => info.DisplayTitle + ", " + info.Start.ConvertTimeFromUtcToUa()));

        return string.IsNullOrWhiteSpace(mathString)
            ? "Sorry. I don't know when will next next matches"
            : $"Next 3 matches :{Environment.NewLine}{mathString}";
    }
}