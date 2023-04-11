using System;
using FluentAssertions;
using Football.Bot.Extensions;
using Xunit;

namespace Football.Bot.UnitTests;

public class DateTimeTests
{
    [Fact]
    public void ShouldConvertTimeToUa()
    {
        var utcNow = new DateTime(2023, 04, 07, 15, 0, 0);
        var expected = new DateTime(2023, 04, 07, 18, 0, 0);
        
        var actual = utcNow.ConvertTimeFromUtcToUa();

        actual.Should().Be(expected);
    }

    [Fact]
    public void ShouldConvertTimeToUtc()
    {
        var ukNow = new DateTime(2023, 04, 07, 15, 0, 0);
        var expected = new DateTime(2023, 04, 07, 14, 0, 0);
        
        var actual = ukNow.ConvertTimeFromUkToUtc();

        actual.Should().Be(expected);
    }
}