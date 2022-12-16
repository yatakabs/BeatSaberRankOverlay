namespace RankOverlay.Scores.Snapshots;

public record SnapshotMetadata
{
    public string? Description { get; init; }

    public Dictionary<string, string> Annotations { get; init; } = new Dictionary<string, string>();
    public Dictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();
}
