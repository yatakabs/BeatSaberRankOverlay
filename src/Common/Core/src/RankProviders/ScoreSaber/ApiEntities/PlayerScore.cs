namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record PlayerScore : ScoreSaberEntityBase
{
    public required Score Score { get; init; }
    public required LeaderBoardInfo LeaderBoard { get; init; }
}
