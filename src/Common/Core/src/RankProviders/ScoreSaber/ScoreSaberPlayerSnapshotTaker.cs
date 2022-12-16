using RankOverlay.PlayerScores;
using RankOverlay.Scores.Snapshots;

namespace RankOverlay.RankProviders.ScoreSaber;

public class ScoreSaberPlayerSnapshotTaker : ISnapshotTaker
{
    private IScoreSaberApiClient ApiClient { get; }
    private PlayerProfile PlayerProfile { get; }
    private string ScoreSaberPlayerId { get; }

    public ScoreSaberPlayerSnapshotTaker(
        string scoreSaberPlayerId,
        PlayerProfile playerProfile,
        IScoreSaberApiClient apiClient)
    {
        this.ApiClient = apiClient;
        this.PlayerProfile = playerProfile;
        this.ScoreSaberPlayerId = scoreSaberPlayerId;
    }

    public async Task<PlayerScoreSnapshot> TakeSnapshotAsync(PlayerScoreSnapshotId id, Func<PlayerScoreSnapshot, CancellationToken, Task<PlayerScoreSnapshot>>? configure = null, CancellationToken cancellationToken = default)
    {
        var player = await this.ApiClient
            .GetPlayerAsync(this.ScoreSaberPlayerId, cancellationToken)
            .ConfigureAwait(false);

        var snapshot = new ScoreSaberPlayerSnapshot
        {
            Id = id,
            PlayerId = this.PlayerProfile.Id,
            RankPlatformPlayerId = this.ScoreSaberPlayerId,
            ScoreSaberPlayerProfile = player,
            Timestamp = DateTimeOffset.Now,
        };

        return configure is null
            ? snapshot
            : await configure
                .Invoke(snapshot, cancellationToken)
                .ConfigureAwait(false);
    }
}
