namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record Player : ScoreSaberEntityBase
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string ProfilePicture { get; init; }
    public required string Country { get; init; }
    public required double Pp { get; init; }
    public required int Rank { get; init; }
    public required int CountryRank { get; init; }
    public required string Role { get; init; }
    public Badge[]? Badges { get; init; }
    public required string Histories { get; init; }
    public ScoreStats? ScoreStats { get; init; }
    public required int Permissions { get; init; }
    public required bool Banned { get; init; }
    public required bool Inactive { get; init; }
}
