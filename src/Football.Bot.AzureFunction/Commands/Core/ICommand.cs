using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Football.Bot.Commands.Core;

public interface ICommand
{
    bool CanExecute(Message message);

    Task Execute(Message message);
}