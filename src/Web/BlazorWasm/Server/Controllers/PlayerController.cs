using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;

namespace RankOverlay.Web.BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/players")]
public class PlayerController : ControllerBase
{
    private IScoreSaberApiClient ScoreSaberApiClient { get; }
    private IPlayerRepository PlayerRepository { get; }

    public PlayerController(
        IScoreSaberApiClient scoreSaberApiClient,
        IPlayerRepository playerRepository)
    {
        this.ScoreSaberApiClient = scoreSaberApiClient;
        this.PlayerRepository = playerRepository;
    }

    [HttpGet("{playerId}")]
    public async Task<PlayerProfile> GetPlayer(
        [FromRoute, Required] PlayerId playerId,
        CancellationToken cancellationToken)
    {
        var player = await this.PlayerRepository
            .GetPlayerProfileOrDefaultAsync(playerId, cancellationToken);

        return player
            ?? throw new BadHttpRequestException("Player not found.", StatusCodes.Status404NotFound);
    }

    [HttpPost]
    public async Task<PlayerProfile> CreatePlayerAsync(
        [FromBody] CreatePlayerParameters playerCreationParameters,
        [FromServices] IScoreSaberApiClient scoreSaberApiClient,
        CancellationToken cancellationToken)
    {
        var profile = await this.PlayerRepository
            .GetPlayerProfileFromRankPlatformIdOrDefaultAsync(
                RankPlatformId.WellKnownPlatforms.ScoreSaber,
                playerCreationParameters.ScoreSaberId,
                (_, _, _) => ValueTask.FromResult<PlayerProfile?>(null),
                cancellationToken);

        return profile is not null
            ? profile
            : await this.PlayerRepository
                .CreatePlayerProfileAsync(
                    async (id, ct) =>
                    {
                        var scoreSaberProfile = await scoreSaberApiClient
                            .GetPlayerAsync(playerCreationParameters.ScoreSaberId, ct);

                        return new PlayerProfile
                        {
                            Id = id,
                            DisplayName = scoreSaberProfile.Name,
                            PlatformProfiles = new Dictionary<RankPlatformId, PlatformPlayerProfile>
                            {
                                [RankPlatformId.WellKnownPlatforms.ScoreSaber] = new ScoreSaberPlayerProfile
                                {
                                    PlatformPlayerId = scoreSaberProfile.Id,
                                    ScoreSaberProfile = scoreSaberProfile,
                                },
                            },
                        };
                    },
                    cancellationToken);
    }

    public record CreatePlayerParameters
    {
        public required string ScoreSaberId { get; init; }
    }

    [HttpGet("{playerId}/platforms/{platformId}/profile")]
    public async Task<Player> GetRankPlatformPlayer(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromServices] IScoreSaberApiClient scoreSaberApiClient,
        CancellationToken cancellationToken)
    {
        if (platformId != RankPlatformId.WellKnownPlatforms.ScoreSaber)
        {
            throw new NotSupportedException("Platform not supported.");
        }

        var profile = await this.PlayerRepository
            .GetPlayerProfileOrDefaultAsync(playerId, cancellationToken);

        if (profile is null)
        {
            throw new BadHttpRequestException("Player not found.", StatusCodes.Status404NotFound);
        }

        if (!profile.PlatformProfiles.TryGetValue(RankPlatformId.WellKnownPlatforms.ScoreSaber, out var scoreSaberProfile))
        {
            throw new BadHttpRequestException("ScoreSaber ID is not registered.");
        }

        var scoreSaberPlayerProfile = await this.ScoreSaberApiClient
            .GetPlayerAsync(
                scoreSaberProfile.PlatformPlayerId,
                cancellationToken);

        return scoreSaberPlayerProfile;
    }
}
