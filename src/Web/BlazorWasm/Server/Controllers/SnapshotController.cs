using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using RankOverlay.PlayerScores;
using RankOverlay.Scores.Snapshots;

namespace RankOverlay.Web.BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/players/{playerId}/platforms/{platformId}/snapshots")]
public class PlayerScoreSnapshotController : ControllerBase
{
    private IPlayerScoreSnapshotRepositoryFactory SnapshotRepositoryFactory { get; }
    private IPlatformPlayerOperations PlatformPlayerOperations { get; }
    private IPlayerRepository PlayerRepository { get; }

    public PlayerScoreSnapshotController(
        IPlayerScoreSnapshotRepositoryFactory snapshotRepositoryFactory,
        IPlatformPlayerOperations platformPlayerOperations,
        IPlayerRepository playerRepository)
    {
        this.SnapshotRepositoryFactory = snapshotRepositoryFactory;
        this.PlatformPlayerOperations = platformPlayerOperations;
        this.PlayerRepository = playerRepository;
    }

    [HttpGet]
    public async IAsyncEnumerable<PlayerScoreSnapshot> ListSnapshotAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromQuery] int? take = 30,
        [FromQuery] int? skip = 0,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        var snapshots = snapshotRepository
            .ListSnapshotAsync(
                new SnapshotListingOptions
                {
                    TakeCount = take,
                    SkipCount = skip,
                    From = from,
                    To = to,
                },
                cancellationToken)
            .WithCancellation(cancellationToken);

        await foreach (var snapshot in snapshots)
        {
            yield return snapshot;
        }
    }

    [HttpGet("{snapshotId}")]
    public async Task<PlayerScoreSnapshot> GetSnapshotAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromRoute, Required] PlayerScoreSnapshotId snapshotId,
        CancellationToken cancellationToken = default)
    {
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        var snapshot = await snapshotRepository
            .GetSnapshotOrDefaultAsync(snapshotId, cancellationToken);

        return snapshot
            ?? throw new BadHttpRequestException("Snapshot not found.", StatusCodes.Status404NotFound);
    }

    [HttpPatch("{snapshotId}/description")]
    public async Task<PlayerScoreSnapshot> UpdateSnapshotDescriptionAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromRoute, Required] PlayerScoreSnapshotId snapshotId,
        [FromBody] SnapshotDescriptionUpdateParameters updateParameters,
        CancellationToken cancellationToken)
    {
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        var snapshot = await snapshotRepository
            .GetSnapshotOrDefaultAsync(snapshotId, cancellationToken);

        return snapshot is null
            ? throw new BadHttpRequestException("Snapshot not found.", StatusCodes.Status404NotFound)
            : await snapshotRepository
            .UpdateSnapshotAsync(
                snapshotId,
                snapshot => snapshot with
                {
                    Metadata = snapshot.Metadata with
                    {
                        Description = updateParameters.Description,
                    },
                },
                cancellationToken);
    }

    [HttpPost]
    public async Task<PlayerScoreSnapshot> CreateSnapshotAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromBody] SnapshotCreationParamters parameters,
        CancellationToken cancellationToken)
    {
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        var player = await this.PlayerRepository.GetPlayerProfileOrDefaultAsync(
            playerId,
            cancellationToken);

        if (player is null)
        {
            throw new BadHttpRequestException(
                "Player not found.",
                StatusCodes.Status404NotFound);
        }

        var snapshotTaker = await this.PlatformPlayerOperations
            .CreatePlayerSnapshotTakerAsync(player, cancellationToken);

        return await snapshotRepository.CreateSnapshotAsync(
            async (id, ct) => await snapshotTaker.TakeSnapshotAsync(
                id,
                snapshot => snapshot with
                {
                    Metadata = snapshot.Metadata with
                    {
                        Description = parameters.Description,
                    }
                },
                ct),
            cancellationToken);
    }

    [HttpPatch("{snapshotId}")]
    public async Task<PlayerScoreSnapshot> UpdateSnapshotAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromRoute, Required] PlayerScoreSnapshotId snapshotId,
        [FromBody] PlayerScoreSnapshotCreationOrUpdateParameters parameters,
        CancellationToken cancellationToken)
    {
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        try
        {
            return await snapshotRepository.UpdateSnapshotAsync(
                snapshotId,
                snapshot => snapshot with
                {
                    Metadata = snapshot.Metadata with
                    {
                        Description = parameters.Description
                            ?? snapshot.Metadata.Description,
                    }
                },
                cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            throw new BadHttpRequestException("Snapshot not found.", StatusCodes.Status404NotFound, ex);
        }
    }

    [HttpDelete("{snapshotId}")]
    public async Task DeleteSnapshotAsync(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromRoute, Required] string snapshotId,
        CancellationToken cancellationToken)
    {
        var playerScoreSnapshotId = PlayerScoreSnapshotId.Parse(snapshotId);
        var snapshotRepository = await this.SnapshotRepositoryFactory
            .GetRepositoryAsync(
                playerId,
                platformId,
                cancellationToken);

        await snapshotRepository
            .DeleteSnapshotAsync(
                playerScoreSnapshotId,
                cancellationToken);
    }

    public record PlayerScoreSnapshotCreationOrUpdateParameters(
        string? Description);

    public record SnapshotCreationParamters(
        string? Description);

    public record SnapshotDescriptionUpdateParameters(
        string Description);
}
