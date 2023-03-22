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

    private const string PartitionId = "61dba35b-4f02-45c5-b648-c6badc0cbd79";

    public CosmosDbClient(Container container) => _container = container;

    public async Task Add(MatchInfo[] matches, string team)
    {
        // Create new object and upsert (create or replace) to container
        MathEntity customItem = new(
            id: Guid.NewGuid().ToString(),
            categoryId: PartitionId,
            numbers: new[] {1, 2, 3},
            matches: matches,
            team: team
        );

        MathEntity createdItem = await _container.CreateItemAsync(
            item: customItem,
            partitionKey: new PartitionKey(PartitionId)
        );

        Console.WriteLine($"Created item:\t{createdItem.id}\t[{createdItem.categoryId}]");
    }

    public async Task<MatchInfo[]> Get(string team)
    {
        // Point read item from container using the id and partitionKey
        var matches = await _container.GetItemLinqQueryable<MathEntity>()
            .Where(entity => entity.team == team)
            .ToListAsync();

        return matches
            .SelectMany(entity => entity.matches)
            .ToArray();
    }
}

public record MathEntity(string id, int[] numbers, string categoryId, string team, MatchInfo[] matches);