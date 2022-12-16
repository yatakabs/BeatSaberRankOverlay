using RankOverlay.PlayerScores;

namespace RankOverlay.Scores.Snapshots;

public static class PlayerScoreSnapshotRepositoryExtensions
{
    public static IAsyncEnumerable<PlayerScoreSnapshot> ListSnapshotAsync(
        this IPlayerScoreSnapshotRepository repository,
        CancellationToken cancellationToken = default)
    {
        return repository.ListSnapshotAsync(
            options: SnapshotListingOptions.Default,
            cancellationToken: cancellationToken);
    }

    public static Task<PlayerScoreSnapshot?> GetSnapshotOrDefaultAsync(
        this IPlayerScoreSnapshotRepository repository,
        PlayerScoreSnapshotId snapshotId,
        CancellationToken cancellationToken = default)
    {
        return repository.GetSnapshotOrDefaultAsync(
            snapshotId,
            _ => null,
            cancellationToken);
    }

    public static Task<PlayerScoreSnapshot> UpdateSnapshotAsync(
        this IPlayerScoreSnapshotRepository repository,
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshot, ValueTask<PlayerScoreSnapshot>> update,
        CancellationToken cancellationToken)
    {
        return repository.UpdateSnapshotAsync(
            snapshotId,
            (snapshot, _) => update.Invoke(snapshot),
            cancellationToken);
    }

    public static Task<PlayerScoreSnapshot> UpdateSnapshotAsync(
        this IPlayerScoreSnapshotRepository repository,
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshot, PlayerScoreSnapshot> update,
        CancellationToken cancellationToken)
    {
        return repository.UpdateSnapshotAsync(
            snapshotId,
            (snapshot, _) => ValueTask.FromResult(update.Invoke(snapshot)),
            cancellationToken);
    }

    public static Task<PlayerScoreSnapshot> CreateSnapshotAsync(
        this IPlayerScoreSnapshotRepository repository,
        Func<PlayerScoreSnapshotId, ValueTask<PlayerScoreSnapshot>> create,
        CancellationToken cancellationToken)
    {
        return repository.CreateSnapshotAsync(
            (snapshot, _) => create.Invoke(snapshot),
            cancellationToken);
    }

    public static Task<PlayerScoreSnapshot> CreateSnapshotAsync(
        this IPlayerScoreSnapshotRepository repository,
        Func<PlayerScoreSnapshotId, PlayerScoreSnapshot> create,
        CancellationToken cancellationToken)
    {
        return repository.CreateSnapshotAsync(
            (snapshot, _) => ValueTask.FromResult(create.Invoke(snapshot)),
            cancellationToken);
    }
}
