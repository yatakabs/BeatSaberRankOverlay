namespace RankOverlay.Configurations;

public interface IConfigurationStorage
{
    Task<GlobalConfigurations> GetGlobalConfigurations(
        CancellationToken cancellationToken = default);

    Task<GlobalConfigurations> UpdateGlobalConfigurationsAsync(
        Func<GlobalConfigurations, CancellationToken, ValueTask<GlobalConfigurations>> update,
        CancellationToken cancellationToken);

    Task<PlayerConfigurations> GetPlayerConfigurations(
        PlayerId playerId,
        CancellationToken cancellationToken = default);

    Task<PlayerConfigurations> UpdatePlayerConfigurationsAsync(
        PlayerId playerId,
        Func<PlayerConfigurations, CancellationToken, ValueTask<PlayerConfigurations>> update,
        CancellationToken cancellationToken);

}
