using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Football.Bot.Commands.Core;

public class CommandHandler
{
    private readonly IEnumerable<ICommand> _commands;

    public CommandHandler(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }

    public async Task Execute(Message message)
    {
        if (!IsValidCommand(message.Text ?? throw new ArgumentNullException(nameof(message.Text))))
        {
            return;
        }

        foreach (var command in _commands.Where(command => command.CanExecute(message)))
        {
            await command.Execute(message);
            break;
        }
    }

    private static bool IsValidCommand(string command) => command.StartsWith('/');
}