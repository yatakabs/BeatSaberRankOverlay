using RankOverlay.Scores.Snapshots;

namespace RankOverlay.Web.BlazorWasm.Server.BlobStorage;

public class BlobStorageScoreSnapshotRepositoryFactory : IPlayerScoreSnapshotRepositoryFactory
{
    private IServiceProvider ServiceProvider { get; }

    public BlobStorageScoreSnapshotRepositoryFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    public ValueTask<IPlayerScoreSnapshotRepository> GetRepositoryAsync(
        PlayerId playerId,
        RankPlatformId platformId,
        CancellationToken cancellationToken = default)
    {
        var repository = ActivatorUtilities
            .CreateInstance<ScoreSaberBlobStoragePlayerScoreSnapshotRepository>(
                this.ServiceProvider,
                playerId,
                platformId);

        return ValueTask.FromResult<IPlayerScoreSnapshotRepository>(repository);
    }
}
