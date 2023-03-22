using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Linq;

namespace Football.Bot.Extensions;

public static class AsyncExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> matches,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        await foreach (var item in matches.ToAsyncFeed(cancellationToken).WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            results.Add(item);
        }

        return results;
    }

    public static async IAsyncEnumerable<T> ToAsyncFeed<T>(this IQueryable<T> matches,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Convert to feed iterator
        using var linqFeed = matches.ToFeedIterator();

        // Iterate query result pages
        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync(cancellationToken);

            // Iterate query results
            foreach (var item in response)
            {
                yield return item;
            }
        }
    }
}