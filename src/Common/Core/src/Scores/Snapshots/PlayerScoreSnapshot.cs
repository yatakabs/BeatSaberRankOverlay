using RankOverlay.PlayerScores;
namespace RankOverlay.Scores.Snapshots;

public abstract record PlayerScoreSnapshot
{
    public required PlayerId PlayerId { get; init; }
    public RankPlatformId PlatformId { get; init; }

    public SnapshotMetadata Metadata { get; init; } = new SnapshotMetadata();

    public required string RankPlatformPlayerId { get; init; }
    public required PlayerScoreSnapshotId Id { get; init; }
    public required DateTimeOffset Timestamp { get; init; }

    protected PlayerScoreSnapshot(RankPlatformId platformId)
    {
        this.PlatformId = platformId;
    }
}
