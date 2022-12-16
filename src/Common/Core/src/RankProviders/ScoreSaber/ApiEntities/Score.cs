namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record Score : ScoreSaberEntityBase
{
    public required long Id { get; init; }
    public LeaderBoardPlayer? LeaderBoardPlayerInfo { get; init; }
    public required long Rank { get; init; }
    public required long BaseScore { get; init; }
    public required long ModifiedScore { get; init; }
    public required double Pp { get; init; }
    public required double Weight { get; init; }
    public required string Modifiers { get; init; }
    public required double Multiplier { get; init; }
    public required long BadCuts { get; init; }
    public required long MissedNotes { get; init; }
    public required long MaxCombo { get; init; }
    public required bool FullCombo { get; init; }
    public required int Hmd { get; init; }
    public required bool HasReplay { get; init; }
    public required DateTimeOffset TimeSet { get; init; }
}
