using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Football.Bot.Extensions;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS8618

namespace Football.Bot.Services;

public record MatchInfo(DateTime Start, string DisplayTitle, string Status);

public class SchedulerProvider
{
    private readonly HttpClient _httpClient;

    public SchedulerProvider(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<MatchInfo[]> GetNextMatches()
    {
        var response = await _httpClient.GetAsync(
            "https://www.chelseafc.com/en/api/fixtures/upcoming?pageId=30EGwHPO9uwBCc75RQY6kg");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to retrieve upcoming fixtures from Chelsea API. Status code: {response.StatusCode}");
        }

        var chelseaMatches = await response.Content.ReadFromJsonAsync<MatchesResponse>() ??
                             throw new ArgumentException();

        var grouped = chelseaMatches.items.SelectMany(x => x.items)
            .GroupBy(match => DateTime.Parse(match.kickoffDate + " " + match.kickoffTime).ConvertTimeFromUkToUtc())
            .SelectMany(items => items.Select(match => DisplayName(match, items.Key)))
            .Select(match => match with {Start = match.Start.ToUniversalTime()})
            .ToArray();

        return grouped;
    }

    private static MatchInfo DisplayName(MatchItem matchItem, DateTime start)
    {
        var displayTitle = matchItem.matchUp.home.clubName + " - " + matchItem.matchUp.away.clubName;

        return new MatchInfo(start, displayTitle, matchItem.status);
    }

    public class MatchesResponse
    {
        public Items[] items { get; set; }
    }

    public class Items
    {
        public MatchItem[] items { get; set; }
    }

    public class MatchItem
    {
        public MatchUp matchUp { get; set; }
        public string kickoffDate { get; set; }
        public string kickoffTime { get; set; }
        public string status { get; set; }
    }

    public class MatchUp
    {
        public Home home { get; set; }
        public Away away { get; set; }
    }

    public class Home
    {
        public string clubName { get; set; }
    }

    public class Away
    {
        public string clubName { get; set; }
    }
}