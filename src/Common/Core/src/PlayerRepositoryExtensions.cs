namespace RankOverlay;

public static class PlayerRepositoryExtensions
{
    public static Task<PlayerProfile?> GetPlayerProfileOrDefaultAsync(
        this IPlayerRepository repository,
        PlayerId id,
        Func<PlayerId, ValueTask<PlayerProfile?>> defaultValueProvider,
        CancellationToken cancellationToken = default)
    {
        return repository
            .GetPlayerProfileOrDefaultAsync(
                id,
                (id, _) => defaultValueProvider.Invoke(id),
                cancellationToken);
    }

    public static Task<PlayerProfile?> GetPlayerProfileOrDefaultAsync(
        this IPlayerRepository repository,
        PlayerId id,
        Func<PlayerId, PlayerProfile?> defaultValueProvider,
        CancellationToken cancellationToken = default)
    {
        return repository
            .GetPlayerProfileOrDefaultAsync(
                id,
                (id, _) => ValueTask.FromResult(defaultValueProvider.Invoke(id)),
                cancellationToken);
    }

    public static Task<PlayerProfile?> GetPlayerProfileOrDefaultAsync(
        this IPlayerRepository repository,
        PlayerId id,
        CancellationToken cancellationToken = default)
    {
        return repository
            .GetPlayerProfileOrDefaultAsync(
                id,
                (id, _) => ValueTask.FromResult<PlayerProfile?>(null),
                cancellationToken);
    }
}
