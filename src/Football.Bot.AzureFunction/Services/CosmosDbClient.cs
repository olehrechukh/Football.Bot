using System;
using System.Linq;
using System.Threading.Tasks;
using Football.Bot.Extensions;
using Microsoft.Azure.Cosmos;

// ReSharper disable InconsistentNaming

namespace Football.Bot.Services;

public class CosmosDbClient
{
    private readonly Container _container;

    public CosmosDbClient(Container container) => _container = container;

    public async Task Add(MatchInfo[] matches, string team)
    {
        // Create new object and upsert (create or replace) to container
        MathEntity customItem = new(
            id: Guid.NewGuid().ToString(),
            categoryId: Constants.PartitionId,
            matches: matches,
            team: team
        );

        MathEntity createdItem = await _container.CreateItemAsync(
            item: customItem,
            partitionKey: new PartitionKey(Constants.PartitionId)
        );

        Console.WriteLine($"Created item:\t{createdItem.id}\t[{createdItem.categoryId}]");
    }

    public async Task<MatchInfo[]> Get(string team)
    {
        // Point read item from container using the id and partitionKey
        var matches = await _container.GetItemLinqQueryable<MathEntity>()
            .Where(entity => entity.team == team)
            .OrderByDescending(entity => entity._ts)
            .ToListAsync();

        return matches
            .SelectMany(entity => entity.matches)
            .ToArray();
    }
}

public record MathEntity(string id, string categoryId, string team, MatchInfo[] matches, long _ts = 0);