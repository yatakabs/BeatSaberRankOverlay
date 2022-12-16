using Microsoft.AspNetCore.Mvc;
using RankOverlay.Configurations;
using RankOverlay.PlayerScores;

namespace RankOverlay.Web.BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/configurations")]
public class ConfigrationsController : ControllerBase
{
    private IConfigurationStorage ConfigurationStorage { get; }

    public ConfigrationsController(IConfigurationStorage configurationStorage)
    {
        this.ConfigurationStorage = configurationStorage;
    }

    public record SetDefaultConfigParameters
    {
        public PlayerId? DefaultPlayerId { get; init; }
    }

    public record SetPlayerConfigParameters
    {
        public RankPlatformId? DefaultPlatformId { get; init; }
        public PlayerScoreSnapshotId? DefaultSnapshotId { get; init; }
    }

    [HttpGet("global")]
    public async Task<GlobalConfigurations> GetGlobalConfigurationsAsync(
    CancellationToken cancellationToken)
    {
        return await this.ConfigurationStorage
            .GetGlobalConfigurations(cancellationToken);
    }

    [HttpPut("global")]
    public async Task<GlobalConfigurations> UpdateGlobalConfigurationsAsync(
        [FromBody] SetDefaultConfigParameters parameters,
        CancellationToken cancellationToken)
    {
        return await this.ConfigurationStorage.UpdateGlobalConfigurationsAsync(
            (conf, _) =>
            {
                var newConfig = conf with
                {
                    DefaultPlayerId = parameters.DefaultPlayerId ?? conf.DefaultPlayerId,
                };

                return ValueTask.FromResult(newConfig);
            },
            cancellationToken);
    }

    [HttpGet("players/{playerId}")]
    public async Task<PlayerConfigurations> GetPlayerConfigurations(
        [FromRoute] PlayerId playerId,
        CancellationToken cancellationToken)
    {
        return await this.ConfigurationStorage
            .GetPlayerConfigurations(playerId, cancellationToken);
    }

    [HttpPut("players/{playerId}")]
    public async Task<PlayerConfigurations> UpdatePlayerConfigurations(
        [FromRoute] PlayerId playerId,
        [FromBody] SetPlayerConfigParameters parameters,
        CancellationToken cancellationToken)
    {
        return await this.ConfigurationStorage.UpdatePlayerConfigurationsAsync(
            playerId,
            (conf, _) =>
            {
                var newConfig = conf with
                {
                    PlayerId = string.IsNullOrEmpty(conf.PlayerId) ? playerId : conf.PlayerId,
                    DefaultSnapshotId = parameters.DefaultSnapshotId ?? conf.DefaultSnapshotId,
                    DefaultPlatformId = parameters.DefaultPlatformId ?? conf.DefaultPlatformId,
                };

                return ValueTask.FromResult(newConfig);
            },
            cancellationToken);
    }
}
