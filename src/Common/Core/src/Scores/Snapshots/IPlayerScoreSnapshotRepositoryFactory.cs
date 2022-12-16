namespace RankOverlay.Scores.Snapshots;

public interface IPlayerScoreSnapshotRepositoryFactory
{
    ValueTask<IPlayerScoreSnapshotRepository> GetRepositoryAsync(
        PlayerId playerId,
        RankPlatformId platformId,
        CancellationToken cancellationToken = default);
}
