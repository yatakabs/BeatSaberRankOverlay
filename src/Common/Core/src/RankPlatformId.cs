using UnitGenerator;

namespace RankOverlay;

[UnitOf(typeof(string), UnitGenerateOptions.JsonConverter | UnitGenerateOptions.JsonConverterDictionaryKeySupport | UnitGenerateOptions.ParseMethod)]
public readonly partial struct RankPlatformId
{
    public RankPlatformId()
    {
        this.value = string.Empty;
    }

    #region Well-known platforms

    public static class WellKnownPlatforms
    {
        public static RankPlatformId ScoreSaber { get; } = (RankPlatformId)nameof(ScoreSaber).ToLower();
        public static RankPlatformId HitBloq { get; } = (RankPlatformId)nameof(HitBloq).ToLower();
        public static RankPlatformId BeatLeader { get; } = (RankPlatformId)nameof(BeatLeader).ToLower();
        public static RankPlatformId BeatFlare { get; } = (RankPlatformId)nameof(BeatFlare).ToLower();

        #endregion

        #region Accessors

        public static RankPlatformId[] WellKnownValues { get; } = new RankPlatformId[] {
        ScoreSaber,
        HitBloq,
        BeatLeader,
        BeatFlare,
    };

        #endregion
    }
}
