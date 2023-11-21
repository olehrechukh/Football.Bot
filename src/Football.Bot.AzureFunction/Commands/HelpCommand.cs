using System.Threading.Tasks;
using Football.Bot.Commands.Core;

namespace Football.Bot.Commands;

internal class HelpCommand : TextBasedCommand
{
    protected override string Pattern => "help";

    private const string HelpingMessage =
        "Please start typing with '/' or press the menu button to view the available options.";

    private readonly TelegramBotClient telegramClient;

    public HelpCommand(TelegramBotClient telegramClient) => this.telegramClient = telegramClient;

    public override async Task Execute(Message message) =>
        await telegramClient.SendTextMessageAsync(message.Chat, HelpingMessage);
}
