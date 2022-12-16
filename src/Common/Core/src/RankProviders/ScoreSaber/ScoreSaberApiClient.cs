using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using FluentUri;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;

namespace RankOverlay.RankProviders.ScoreSaber;

public class ScoreSaberApiClient : IScoreSaberApiClient
{
    public static string HttpClientName { get; } = nameof(ScoreSaberApiClient);

    private IHttpClientFactory HttpClientFactory { get; }

    public ScoreSaberApiClient(IHttpClientFactory httpClientFactory)
    {
        this.HttpClientFactory = httpClientFactory;
    }

    public async Task<Player> GetPlayerAsync(
        string playerId,
        CancellationToken cancellationToken = default)
    {
        using var httpClient = this.HttpClientFactory.CreateClient(HttpClientName);

        var player = await httpClient.GetFromJsonAsync<Player>(
            $"api/player/{playerId}/full",
            cancellationToken);
        return player!;
    }

    public async IAsyncEnumerable<PlayerScore> GetPlayerScoresAsync(
        string playerId,
        ScoreSortType sort,
        int pageSize = 50,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpClient = this.HttpClientFactory.CreateClient(HttpClientName);

        var uriBuilderBase = FluentUriBuilder.Create()
            .Path($"api/player/{playerId}/scores")
            .QueryParam("limit", pageSize)
            .QueryParam("sort", sort.ToString().ToLower())
            .QueryParam("includeMetadata", "true");

        for (int page = 1, totalCount = 0; ; page++)
        {
            var uri = uriBuilderBase
                .QueryParam("page", page)
                .ToUri()
                .PathAndQuery;

            var collection = await httpClient.GetFromJsonAsync<PlayerScoreCollection>(uri, cancellationToken);

            if (collection == null)
            {
                break;
            }

            foreach (var score in collection.PlayerScores)
            {
                yield return score;
            }

            totalCount += collection.PlayerScores.Length;

            if (totalCount >= collection.Metadata.Total || collection.PlayerScores.Length == 0)
            {
                break;
            }
        }
    }
}
