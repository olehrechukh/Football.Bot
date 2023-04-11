using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Football.Bot.Extensions;

// ReSharper disable InconsistentNaming

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

        var chelseaMatches = await response.Content.ReadFromJsonAsync<RootObject>();

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

    public class RootObject
    {
        public Items[] items { get; set; }
        public Competitions[] competitions { get; set; }
    }

    public class Items
    {
        public string id { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string monthName { get; set; }
        public MatchItem[] items { get; set; }
    }

    public class MatchItem
    {
        public string id { get; set; }
        public string optaId { get; set; }
        public bool isResult { get; set; }
        public MatchUp matchUp { get; set; }
        public string venue { get; set; }
        public string competition { get; set; }
        public string kickoffDate { get; set; }
        public string kickoffTime { get; set; }
        public bool isLive { get; set; }
        public string status { get; set; }
        public bool tbc { get; set; }
        public bool postponed { get; set; }
        public Ctas ctas { get; set; }
        public Labels labels { get; set; }
    }

    public class MatchUp
    {
        public bool isResult { get; set; }
        public bool isLive { get; set; }
        public bool isHomeFixture { get; set; }
        public string status { get; set; }
        public Home home { get; set; }
        public Away away { get; set; }
        public string kickoffTime { get; set; }
        public bool tbc { get; set; }
        public bool postponed { get; set; }
    }

    public class Home
    {
        public string clubName { get; set; }
        public string clubShortName { get; set; }
        public string clubCrestUrl { get; set; }
        public int score { get; set; }
    }

    public class Away
    {
        public string clubName { get; set; }
        public string clubShortName { get; set; }
        public string clubCrestUrl { get; set; }
        public int score { get; set; }
    }

    public class Ctas
    {
        public MatchCentreLink matchCentreLink { get; set; }
    }

    public class MatchCentreLink
    {
        public string id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public bool isExternal { get; set; }
        public bool isActive { get; set; }
    }

    public class Labels
    {
        public string abandonedLabel { get; set; }
        public string postponedLabel { get; set; }
        public string canceledLabel { get; set; }
        public string tbcLabel { get; set; }
    }

    public class Competitions
    {
        public string displayText { get; set; }
        public bool selectedValue { get; set; }
        public string value { get; set; }
    }


    public class ChelseaMatch
    {
        public DateTime Date { get; set; }
    }
}