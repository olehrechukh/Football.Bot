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

    private readonly TelegramBotClient telegramClient;

    public StartCommand(TelegramBotClient telegramClient) => this.telegramClient = telegramClient;

    public override async Task Execute(Message message) =>
        await telegramClient.SendTextMessageAsync(message.Chat, GreetingMessage);
}