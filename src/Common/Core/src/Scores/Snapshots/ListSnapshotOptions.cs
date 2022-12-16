namespace RankOverlay.Scores.Snapshots;

public record SnapshotListingOptions
{
    public int? SkipCount { get; init; }
    public int? TakeCount { get; init; }

    /// <remarks>Inclusive</remarks>
    public DateTimeOffset? From { get; init; }

    /// <remarks>Exclusive</remarks>
    public DateTimeOffset? To { get; init; }

    public static SnapshotListingOptions Default { get; } = new SnapshotListingOptions();
}
