using RankOverlay.RankProviders.ScoreSaber.ApiEntities;

namespace RankOverlay;

public record PlatformPlayerProfile
{
    public RankPlatformId PlatformId { get; init; }
    public required string PlatformPlayerId { get; init; }
}

public record ScoreSaberPlayerProfile : PlatformPlayerProfile
{
    public ScoreSaberPlayerProfile()
    {
        this.PlatformId = RankPlatformId.WellKnownPlatforms.ScoreSaber;
    }

    public required Player ScoreSaberProfile { get; init; }
}
