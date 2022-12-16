using Microsoft.Extensions.Options;
using Storage.Net.Blobs;

namespace RankOverlay.Web.BlazorWasm.Server.BlobStorage;

public class BlobPlayerRepository : IPlayerRepository
{
    private IBlobStorage BlobStorage { get; }
    private BlobStorageOptions BlobStorageOptions { get; }

    public BlobPlayerRepository(
        IBlobStorage blobStorage,
        IOptionsSnapshot<BlobStorageOptions> blobStorageOptions)
    {
        this.BlobStorage = blobStorage;
        this.BlobStorageOptions = blobStorageOptions.Value;
    }

    #region Helper methods

    private string GetBaseFolderPath()
    {
        return "players";
    }

    private string GetPlayerProfilePath(PlayerId playerId)
    {
        return Path.Combine(
            this.GetBaseFolderPath(),
            playerId,
            "profile.json");
    }

    #endregion

    public async Task<PlayerProfile> CreatePlayerProfileAsync(
        Func<PlayerId, CancellationToken, ValueTask<PlayerProfile>> create,
        CancellationToken cancellationToken = default)
    {
        var playerId = PlayerId.New();
        var path = this.GetPlayerProfilePath(playerId);

        var playerProfile = await create
            .Invoke(playerId, cancellationToken)
            .ConfigureAwait(false);

        if (playerProfile.Id != playerId)
        {
            throw new InvalidOperationException("Given snapshot ID must be set.");
        }

        await this.BlobStorage
            .WriteJsonAsync(
                fullPath: path,
                instance: playerProfile,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return playerProfile;
    }

    public async Task<PlayerProfile?> GetPlayerProfileOrDefaultAsync(
        PlayerId playerId,
        Func<PlayerId, CancellationToken, ValueTask<PlayerProfile?>> defaultValueProvider,
        CancellationToken cancellationToken = default)
    {
        var path = this.GetPlayerProfilePath(playerId);

        var playerProfile = await this.BlobStorage
            .ReadJsonAsync<PlayerProfile>(
                fullPath: path,
                ignoreInvalidJson: true,
                options: this.BlobStorageOptions.JsonSerializerOptions,
                encoding: this.BlobStorageOptions.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return playerProfile
            ?? await defaultValueProvider
                .Invoke(playerId, cancellationToken)
                .ConfigureAwait(false);
    }

    public async Task<PlayerProfile?> GetPlayerProfileFromRankPlatformIdOrDefaultAsync(
        RankPlatformId platformId,
        string rankPlatformPlayerId,
        Func<RankPlatformId, string, CancellationToken, ValueTask<PlayerProfile?>> defaultValueProvider,
        CancellationToken cancellationToken = default)
    {
        var profile = await this.BlobStorage
            .ListFilesAsync(
                new ListOptions
                {
                    FolderPath = this.GetBaseFolderPath(),
                    Recurse = true,
                },
                cancellationToken)
            .ToAsyncEnumerable()
            .SelectMany(x => x.ToAsyncEnumerable())
            .SelectAwaitWithCancellation(async (blob, ct) =>
                await this.BlobStorage
                    .ReadJsonAsync<PlayerProfile>(
                        fullPath: blob.FullPath,
                        ignoreInvalidJson: true,
                        options: this.BlobStorageOptions.JsonSerializerOptions,
                        encoding: this.BlobStorageOptions.TextEncoding,
                        cancellationToken: ct))
            .FirstOrDefaultAsync(x => x.PlatformProfiles.GetValueOrDefault(platformId)?.PlatformPlayerId == rankPlatformPlayerId)
            .ConfigureAwait(false);

        return profile
            ?? await defaultValueProvider
                .Invoke(platformId, rankPlatformPlayerId, cancellationToken)
                .ConfigureAwait(false);
    }
}
