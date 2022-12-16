using RankOverlay.PlayerScores;

namespace RankOverlay.Scores.Snapshots;

public interface ISnapshotTaker
{
    Task<PlayerScoreSnapshot> TakeSnapshotAsync(
        PlayerScoreSnapshotId id,
        Func<PlayerScoreSnapshot, CancellationToken, Task<PlayerScoreSnapshot>>? configure = null,
        CancellationToken cancellationToken = default);
}
