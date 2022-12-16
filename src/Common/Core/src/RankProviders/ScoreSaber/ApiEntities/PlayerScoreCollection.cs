namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record PlayerScoreCollection : ScoreSaberEntityBase
{
    public required PlayerScore[] PlayerScores { get; init; }
    public required PagingMetadata Metadata { get; init; }
}
