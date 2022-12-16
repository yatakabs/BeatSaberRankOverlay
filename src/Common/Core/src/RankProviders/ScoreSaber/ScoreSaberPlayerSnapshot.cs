using RankOverlay.RankProviders.ScoreSaber.ApiEntities;
using RankOverlay.Scores.Snapshots;

namespace RankOverlay.RankProviders.ScoreSaber;

public record ScoreSaberPlayerSnapshot : PlayerScoreSnapshot
{
    public ScoreSaberPlayerSnapshot()
        : base(RankPlatformId.WellKnownPlatforms.ScoreSaber)
    {
    }

    public required Player ScoreSaberPlayerProfile { get; init; }
}
