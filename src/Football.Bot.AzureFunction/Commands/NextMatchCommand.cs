using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Football.Bot.Extensions;
using Football.Bot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Bot.Commands;

internal class UnhandledCommand : ICommand
{
    private readonly TelegramBotClient _telegramClient;

    public UnhandledCommand(TelegramBotClient telegramClient)
    {
        _telegramClient = telegramClient;
    }

    public bool CanExecute(Message message)
    {
        return true;
    }

    public async Task Execute(Message message)
    {
        const string displayString = "Невідома команда, використай існючу команду або запитай @valdoalvarez";
        await _telegramClient.SendTextMessageAsync(message.Chat, displayString);
    }
}

internal class NextMatchCommand : ICommand
{
    private const string Pattern = "/next";

    private readonly CosmosDbClient _cosmosDbClient;
    private readonly TelegramBotClient _telegramClient;

    public NextMatchCommand(CosmosDbClient cosmosDbClient, TelegramBotClient telegramClient)
    {
        _cosmosDbClient = cosmosDbClient;
        _telegramClient = telegramClient;
    }

    public bool CanExecute(Message message)
    {
        return message?.Text == Pattern // direct message
               || message?.Chat.Type == ChatType.Group || message?.Text?.StartsWith(Pattern + "@") == true; // message in group
    }

    public async Task Execute(Message message)
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
            ? "Я не знаю, коли наступний матч, будь ласка, звертайся до @valdoalvarez "
            : $"Наступні 3 матчі:{Environment.NewLine}{mathString}";
    }
}