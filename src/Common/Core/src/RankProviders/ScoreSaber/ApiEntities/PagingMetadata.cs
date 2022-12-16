namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record PagingMetadata : ScoreSaberEntityBase
{
    public required long Total { get; init; }
    public required int Page { get; init; }
    public required int ItemsPerPage { get; init; }
}
