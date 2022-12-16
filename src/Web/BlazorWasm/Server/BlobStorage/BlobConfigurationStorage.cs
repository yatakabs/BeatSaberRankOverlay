using Microsoft.Extensions.Options;
using RankOverlay.Configurations;
using Storage.Net.Blobs;

namespace RankOverlay.Web.BlazorWasm.Server.BlobStorage;

public class BlobConfigurationStorage : IConfigurationStorage
{
    public string DefaultConfigurationPath { get; } = "config/global/default";

    private IBlobStorage BlobStorage { get; }
    private BlobStorageOptions BlobStorageOptions { get; }

    public BlobConfigurationStorage(
        IBlobStorage blobStorage,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
    {
        this.BlobStorage = blobStorage;
        this.BlobStorageOptions = blobStorageOptions.Value;
    }

    public async Task<GlobalConfigurations> GetGlobalConfigurations(
        CancellationToken cancellationToken = default)
    {
        var ret = await this.BlobStorage
            .ReadJsonAsync<GlobalConfigurations>(
                fullPath: this.DefaultConfigurationPath,
                ignoreInvalidJson: true,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return ret ?? new GlobalConfigurations();
    }

    public async Task<GlobalConfigurations> UpdateGlobalConfigurationsAsync(
        Func<GlobalConfigurations, CancellationToken, ValueTask<GlobalConfigurations>> update,
        CancellationToken cancellationToken)
    {
        var config = await this
            .GetGlobalConfigurations(cancellationToken)
            .ConfigureAwait(false);

        var newConfig = await update
            .Invoke(config, cancellationToken)
            .ConfigureAwait(false);

        await this.BlobStorage
            .WriteJsonAsync(
                fullPath: this.DefaultConfigurationPath,
                instance: newConfig,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return newConfig;
    }

    public async Task<PlayerConfigurations> GetPlayerConfigurations(
        PlayerId playerId,
        CancellationToken cancellationToken = default)
    {
        var path = $"config/players/{playerId}";

        var ret = await this.BlobStorage
            .ReadJsonAsync<PlayerConfigurations>(
                fullPath: path,
                ignoreInvalidJson: true,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return ret ?? new PlayerConfigurations
        {
            PlayerId = playerId,
        };
    }

    public async Task<PlayerConfigurations> UpdatePlayerConfigurationsAsync(
        PlayerId playerId,
        Func<PlayerConfigurations, CancellationToken, ValueTask<PlayerConfigurations>> update,
        CancellationToken cancellationToken)
    {
        var path = $"config/players/{playerId}";

        var config = await this
            .GetPlayerConfigurations(playerId, cancellationToken)
            .ConfigureAwait(false);

        var newConfig = await update
            .Invoke(config, cancellationToken)
            .ConfigureAwait(false);

        await this.BlobStorage
            .WriteJsonAsync(
                fullPath: path,
                instance: newConfig,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return newConfig;
    }
}
