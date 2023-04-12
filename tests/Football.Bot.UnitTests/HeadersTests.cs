using System.Collections.Generic;
using FluentAssertions;
using Football.Bot.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Football.Bot.UnitTests;

public class HeadersTests
{
    [Fact]
    public void ShouldCheck_PresentCorrectToken()
    {
        var secret = "123123";
        var headers = new HeaderDictionary(new Dictionary<string, StringValues>
        {
            ["Connection"] = new("keep-alive"),
            ["Accept-Encoding"] = new(new[] {"gzip", "deflate"}),
            ["X-Telegram-Bot-Api-Secret-Token"] = new(secret),
        });

        var matchSecretValue = HeadersValidator.MatchSecretValue(headers, secret);

        matchSecretValue.Should().BeTrue();
    }

    [Fact]
    public void ShouldCheck_PresentIncorrectToken()
    {
        var secret = "123123";
        var headers = new HeaderDictionary(new Dictionary<string, StringValues>
        {
            ["Connection"] = new("keep-alive"),
            ["Accept-Encoding"] = new(new[] {"gzip", "deflate"}),
            ["X-Telegram-Bot-Api-Secret-Token"] = new(secret),
        });

        var matchSecretValue = HeadersValidator.MatchSecretValue(headers, secret + 1);

        matchSecretValue.Should().BeFalse();
    }

    [Fact]
    public void ShouldCheck_MissingToken()
    {
        var secret = "123123";
        var headers = new HeaderDictionary(new Dictionary<string, StringValues>
        {
            ["Connection"] = new("keep-alive"),
            ["Accept-Encoding"] = new(new[] {"gzip", "deflate"}),
        });

        var matchSecretValue = HeadersValidator.MatchSecretValue(headers, secret );

        matchSecretValue.Should().BeFalse();
    }
}