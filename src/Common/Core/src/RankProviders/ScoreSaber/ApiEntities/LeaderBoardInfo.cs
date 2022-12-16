namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record LeaderBoardInfo : ScoreSaberEntityBase
{
    public required long Id { get; init; }
    public required string SongHash { get; init; }
    public required string SongName { get; init; }
    public required string SongSubName { get; init; }
    public required string SongAuthorName { get; init; }
    public required string LevelAuthorName { get; init; }
    public required MapDifficulty Difficulty { get; init; }
    public required long MaxScore { get; init; }
    public required DateTimeOffset CreatedDate { get; init; }
    public required DateTimeOffset? RankedDate { get; init; }
    public required DateTimeOffset? QualifiedDate { get; init; }
    public required DateTimeOffset? LovedDate { get; init; }
    public required bool Ranked { get; init; }
    public required bool Qualified { get; init; }
    public required bool Loved { get; init; }
    public required double MaxPp { get; init; }
    public required double Stars { get; init; }
    public required bool PositiveModifiers { get; init; }
    public required long Plays { get; init; }

    public required long DailyPlays { get; init; }
    public required string CoverImage { get; init; }
    public required Score? PlayerScore { get; init; }
    public required MapDifficulty[]? Difficulties { get; init; }
}
