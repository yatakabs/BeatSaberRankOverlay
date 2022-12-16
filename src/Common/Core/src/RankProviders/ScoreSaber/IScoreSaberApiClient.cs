using RankOverlay.RankProviders.ScoreSaber.ApiEntities;

namespace RankOverlay.RankProviders.ScoreSaber;

public interface IScoreSaberApiClient
{
    Task<Player> GetPlayerAsync(
        string scoreSaberPlayerId,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerScore> GetPlayerScoresAsync(
        string scoreSaberPlayerId,
        ScoreSortType sort,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
