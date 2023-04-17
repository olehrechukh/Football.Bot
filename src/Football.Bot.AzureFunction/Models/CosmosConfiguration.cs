#pragma warning disable CS8618

namespace Football.Bot.Models;

public class CosmosConfiguration
{
    public string Token { get; set; }
    public string Endpoint { get; set; }
    public string Database { get; set; }
    public string Container { get; set; }
}