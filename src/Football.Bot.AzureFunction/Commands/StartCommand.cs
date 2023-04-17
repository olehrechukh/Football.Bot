using System.Threading.Tasks;
using Football.Bot.Commands.Core;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Football.Bot.Commands;

internal class StartCommand : TextBasedCommand
{
    protected override string Pattern => "start";

    private const string GreetingMessage =
        @"An illustration of the serverless football bot can be found at github.com/olehrechukh/telegram-football-bot, which is under the ownership of @olehrechukh. 
To engage with the bot, kindly refer to the available bot commands.";

    private readonly TelegramBotClient _telegramClient;

    public StartCommand(TelegramBotClient telegramClient) => _telegramClient = telegramClient;

    public override async Task Execute(Message message) =>
        await _telegramClient.SendTextMessageAsync(message.Chat, GreetingMessage);
}