using System;

namespace Football.Bot.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ConvertTimeFromUtcToUa(this DateTime dateTime)
    {
        var uaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, uaTimeZone);
    }
    
    public static DateTime ConvertTimeFromUkToUtc(this DateTime dateTime)
    {
        var ukTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        return TimeZoneInfo.ConvertTimeToUtc(dateTime, ukTimeZone);
    }
}