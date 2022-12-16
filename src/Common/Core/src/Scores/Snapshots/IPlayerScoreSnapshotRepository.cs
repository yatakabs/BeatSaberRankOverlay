using RankOverlay.PlayerScores;

namespace RankOverlay.Scores.Snapshots;
public interface IPlayerScoreSnapshotRepository
{
    PlayerId PlayerId { get; }

    Task<PlayerScoreSnapshot> CreateSnapshotAsync(
        Func<PlayerScoreSnapshotId, CancellationToken, ValueTask<PlayerScoreSnapshot>> create,
        CancellationToken cancellationToken);

    Task<PlayerScoreSnapshot> UpdateSnapshotAsync(
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshot, CancellationToken, ValueTask<PlayerScoreSnapshot>> update,
        CancellationToken cancellationToken);

    Task DeleteSnapshotAsync(
        PlayerScoreSnapshotId snapshotId,
        CancellationToken cancellationToken = default);

    Task<PlayerScoreSnapshot?> GetSnapshotOrDefaultAsync(
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshotId, PlayerScoreSnapshot?> defaultValueProvider,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<PlayerScoreSnapshot> ListSnapshotAsync(
        SnapshotListingOptions options,
        CancellationToken cancellationToken = default);
}
