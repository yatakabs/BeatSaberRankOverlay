namespace RankOverlay;

public record PlayerProfile
{
    public required PlayerId Id { get; init; }
    public required string DisplayName { get; init; }

    public required Dictionary<RankPlatformId, PlatformPlayerProfile> PlatformProfiles { get; init; }
}
