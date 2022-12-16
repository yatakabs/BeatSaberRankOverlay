using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;

namespace RankOverlay.Web.BlazorWasm.Server.Controllers;

[ApiController]
[Route("api/players/{playerId}/platforms/{platformId}/scores")]
public class PlayerScoresController : ControllerBase
{
    public static int MaxRecentScores { get; } = 30;

    private IScoreSaberApiClient ScoreSaberApiClient { get; }
    private IPlayerRepository PlayerRepository { get; }

    public PlayerScoresController(
        IScoreSaberApiClient scoreSaberApiClient,
        IPlayerRepository playerRepository)
    {
        this.ScoreSaberApiClient = scoreSaberApiClient;
        this.PlayerRepository = playerRepository;
    }

    [HttpGet("recent/ranked")]
    public async Task<PlayerScore[]> RecentRankedScores(
        [FromRoute, Required] PlayerId playerId,
        [FromRoute, Required] RankPlatformId platformId,
        [FromQuery] int count = 0,
        [FromQuery] DateTimeOffset? since = null,
        CancellationToken cancellationToken = default)
    {
        if (platformId != RankPlatformId.WellKnownPlatforms.ScoreSaber)
        {
            throw new BadHttpRequestException("Platform not supported.");
        }

        var player = await this.PlayerRepository
            .GetPlayerProfileOrDefaultAsync(playerId, cancellationToken);

        if (player is null)
        {
            throw new BadHttpRequestException(
                "Player not found.",
                StatusCodes.Status404NotFound);
        }

        if (!player.PlatformProfiles.TryGetValue(platformId, out var platformProfile))
        {
            throw new BadHttpRequestException(
                "ScoreSaber ID is not set for the player.",
                StatusCodes.Status400BadRequest);
        }

        var scoreSaberPlayerId = platformProfile.PlatformPlayerId;

        var scores = this.ScoreSaberApiClient
            .GetPlayerScoresAsync(
                scoreSaberPlayerId,
                ScoreSortType.Recent,
                cancellationToken: cancellationToken);

        if (since is not null)
        {
            scores = scores
                .TakeWhile(x => x.Score.TimeSet > since);
        }

        if (count <= 0 || count > MaxRecentScores)
        {
            count = MaxRecentScores;
        }

        var result = await scores
            .Where(x => x.LeaderBoard.Ranked)
            .Take(count)
            .ToArrayAsync(cancellationToken);

        return result;
    }
}
