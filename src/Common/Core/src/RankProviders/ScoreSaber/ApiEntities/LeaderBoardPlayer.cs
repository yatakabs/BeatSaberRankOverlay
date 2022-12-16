namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record LeaderBoardPlayer : ScoreSaberEntityBase
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string ProfilePicture { get; init; }
    public required string Country { get; init; }
    public required int Permissions { get; init; }
    public required string Role { get; init; }
}
