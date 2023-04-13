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
        const int count = 3;
        var schedulerProvider = new SchedulerProvider(new HttpClient());

        var nextMatches = await schedulerProvider.GetNextMatches(count);

        nextMatches.Distinct().Should().HaveCount(count);
    }
}