using FluentAssertions;
using Football.Bot.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Football.Bot.IntegrationTests;

public class TestFunctionsTests
{
    [Fact]
    public void ShouldReturnOkStatus()
    {
        var defaultHttpRequest = new DefaultHttpRequest(new DefaultHttpContext());
        var actionResult = TestFunctions.Test(defaultHttpRequest);

        actionResult.Should().BeOfType<OkObjectResult>();

        var response = ((OkObjectResult)actionResult).Value;

        response.Should().BeOfType<TestResponse>();

        ((TestResponse)response).Status.Should().Be("ok");
    }
}
