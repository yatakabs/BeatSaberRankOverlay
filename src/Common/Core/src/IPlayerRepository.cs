namespace RankOverlay;

public interface IPlayerRepository
{
    Task<PlayerProfile?> GetPlayerProfileOrDefaultAsync(
        PlayerId playerId,
        Func<PlayerId, CancellationToken, ValueTask<PlayerProfile?>> defaultValueProvider,
        CancellationToken cancellationToken = default);

    Task<PlayerProfile?> GetPlayerProfileFromRankPlatformIdOrDefaultAsync(
        RankPlatformId platformId,
        string rankPlatformPlayerId,
        Func<RankPlatformId, string, CancellationToken, ValueTask<PlayerProfile?>> defaultValueProvider,
        CancellationToken cancellationToken = default);

    Task<PlayerProfile> CreatePlayerProfileAsync(
        Func<PlayerId, CancellationToken, ValueTask<PlayerProfile>> create,
        CancellationToken cancellationToken = default);
}
