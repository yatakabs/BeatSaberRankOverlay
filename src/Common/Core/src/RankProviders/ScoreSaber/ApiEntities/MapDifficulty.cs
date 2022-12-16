namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record MapDifficulty : ScoreSaberEntityBase
{
    public required long LeaderBoardId { get; init; }
    public required long Difficulty { get; init; }
    public required string GameMode { get; init; }
    public required string DifficultyRaw { get; init; }
    public string DifficultyName => this.Difficulty switch
    {
        1 => "Easy",
        3 => "Normal",
        5 => "Hard",
        7 => "Expert",
        9 => "Expert+",
        _ => "N/A",
    };

    public string DifficultyNameForClass => this.Difficulty switch
    {
        1 => "level-easy",
        3 => "level-normal",
        5 => "level-hard",
        7 => "level-expert",
        9 => "level-expertplus",
        _ => "level-unknown",
    };
}
