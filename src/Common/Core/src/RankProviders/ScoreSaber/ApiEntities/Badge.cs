namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record Badge : ScoreSaberEntityBase
{
    public required string Description { get; init; }
    public required string Image { get; init; }
}
