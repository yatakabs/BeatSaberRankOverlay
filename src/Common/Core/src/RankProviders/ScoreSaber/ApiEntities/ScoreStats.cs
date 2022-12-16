namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record ScoreStats : ScoreSaberEntityBase
{
    public long TotalScore { get; init; }
    public long TotalRankedScore { get; init; }
    public double AverageRankedAccuracy { get; init; }
    public long TotalPlayCount { get; init; }
    public long RankedPlayCount { get; init; }
    public int ReplaysWatched { get; init; }
}
