using FluentAssertions;
using Football.Bot.Functions;
using Football.Bot.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Football.Bot.UnitTests;

public class TelegramUriHelperTests
{
    [Theory]
    [InlineData("http", "example.com", null, "http://example.com/api/webhook")]
    [InlineData("https", "example.com", null, "https://example.com/api/webhook")]
    [InlineData("https", "example.com", 8080, "https://example.com:8080/api/webhook")]
    public void ShouldConvertToTelegramWebhook_ReturnsCorrectUri(string scheme, string host, int? port, string expected)
    {
        // Arrange
        var hostString = port.HasValue ? new HostString(host, port.Value) : new HostString(host);

        // Act
        var result = TelegramUriHelper.ConvertToTelegramWebhook(scheme, hostString);
        
        // Assert
        result.Should().Be(expected);
    }
}