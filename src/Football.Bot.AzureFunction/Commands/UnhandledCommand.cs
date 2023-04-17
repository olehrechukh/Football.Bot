using System;
using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Football.Bot.Commands;

internal class UnhandledCommand : ICommand
{
    private readonly TelegramBotClient _telegramClient;

    public UnhandledCommand(TelegramBotClient telegramClient)
    {
        _telegramClient = telegramClient;
    }

    public bool CanExecute(Message message) => true;

    public async Task Execute(Message message)
    {
        var displayString = $"Unknown command. {Environment.NewLine}. See '/' commands";
        
        await _telegramClient.SendTextMessageAsync(message.Chat, displayString);
    }
}