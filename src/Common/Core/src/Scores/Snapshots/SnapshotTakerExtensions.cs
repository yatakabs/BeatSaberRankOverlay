using RankOverlay.PlayerScores;

namespace RankOverlay.Scores.Snapshots;

public static class SnapshotTakerExtensions
{
    public static Task<PlayerScoreSnapshot> TakeSnapshotAsync(
        this ISnapshotTaker snapshotTaker,
        PlayerScoreSnapshotId id,
        Func<PlayerScoreSnapshot, Task<PlayerScoreSnapshot>> configure,
        CancellationToken cancellationToken = default)
    {
        return snapshotTaker.TakeSnapshotAsync(
            id,
            (snapshot, _) => configure.Invoke(snapshot),
            cancellationToken);
    }

    public static Task<PlayerScoreSnapshot> TakeSnapshotAsync(
        this ISnapshotTaker snapshotTaker,
        PlayerScoreSnapshotId id,
        Func<PlayerScoreSnapshot, PlayerScoreSnapshot> configure,
        CancellationToken cancellationToken = default)
    {
        return snapshotTaker.TakeSnapshotAsync(
            id,
            (snapshot, _) => Task.FromResult(configure.Invoke(snapshot)),
            cancellationToken);
    }
}
