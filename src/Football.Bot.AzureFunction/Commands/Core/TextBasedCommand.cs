using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Football.Bot.Commands.Core;

public abstract class TextBasedCommand : ICommand
{
    protected abstract string Pattern { get; }

    public bool CanExecute(Message message)
    {
        var pattern = "/" + Pattern;
        
        return message.Text == pattern // direct message
               || message.Chat.Type == ChatType.Group || message.Text?.StartsWith(pattern + "@") == true; // message in group
    }

    public abstract Task Execute(Message message);
}