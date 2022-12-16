using RankOverlay.PlayerScores;

namespace RankOverlay.Configurations;

public record PlayerConfigurations
{
    public PlayerId PlayerId { get; init; }
    public RankPlatformId? DefaultPlatformId { get; init; }
    public PlayerScoreSnapshotId? DefaultSnapshotId { get; init; }
}
