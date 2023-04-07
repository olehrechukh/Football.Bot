using System;

namespace Football.Bot.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ConvertTimeFromUtcToUa(this DateTime dateTime)
    {
        var uaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, uaTimeZone);
    }
}