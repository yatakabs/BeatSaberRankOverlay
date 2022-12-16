namespace RankOverlay.Scores.Snapshots;

public interface IPlatformPlayerOperations
{
    ValueTask<ISnapshotTaker> CreatePlayerSnapshotTakerAsync(
        PlayerProfile player,
        CancellationToken cancellationToken = default);
}
