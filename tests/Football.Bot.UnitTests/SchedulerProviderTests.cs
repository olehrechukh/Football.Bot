using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Football.Bot.Services;
using Xunit;

namespace Football.Bot.UnitTests;

public class SchedulerProviderTests
{
    [Fact]
    public async Task ShouldReturnNext3DistinctMatches()
    {
        var schedulerProvider = new SchedulerProvider(new HttpClient());

        var nextMatches = await schedulerProvider.GetNextMatches();

        nextMatches.Distinct().Should().HaveCount(3);
    }
}