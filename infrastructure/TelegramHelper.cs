using System;
using System.Text;

namespace Football.Bot.Infrastructure;

public class TelegramHelper
{
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
    private const int Length = 32;

    /// <summary>
    /// The function generates a string 32 characters long, and can only contain characters from the set of
    /// uppercase and lowercase letters (A-Z and a-z), numbers (0-9), underscore (_), and hyphen (-).
    /// </summary>
    /// <returns>Random 32 characters long string.</returns>
    public static string GenerateSecretToken()
    {
        var result = new StringBuilder();
        var random = new Random();

        for (int i = 0; i < Length; i++)
        {
            result.Append(AllowedChars[random.Next(AllowedChars.Length)]);
        }

        return result.ToString();
    }
}