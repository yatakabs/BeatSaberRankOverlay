using Microsoft.Extensions.DependencyInjection;
using RankOverlay.Scores.Snapshots;

namespace RankOverlay.RankProviders.ScoreSaber;

public class ScoreSaberPlatformPlayerOperations : IPlatformPlayerOperations
{
    private IServiceProvider ServiceProvider { get; }

    public ScoreSaberPlatformPlayerOperations(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    public ValueTask<ISnapshotTaker> CreatePlayerSnapshotTakerAsync(
        PlayerProfile player,
        CancellationToken cancellationToken = default)
    {
        var scoreSaberPlayerId = player.PlatformProfiles.TryGetValue(RankPlatformId.WellKnownPlatforms.ScoreSaber, out var scoreSaberPlatformProfile)
            ? scoreSaberPlatformProfile.PlatformPlayerId
            : throw new KeyNotFoundException("ScoreSaber ID is not registered for the player.");

        var snapshotTaker = ActivatorUtilities.CreateInstance<ScoreSaberPlayerSnapshotTaker>(
            this.ServiceProvider,
            player,
            scoreSaberPlayerId);

        return ValueTask.FromResult<ISnapshotTaker>(snapshotTaker);
    }
}
