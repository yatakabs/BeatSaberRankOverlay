using UnitGenerator;

namespace RankOverlay.PlayerScores;

[UnitOf(typeof(Ulid), UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.JsonConverterDictionaryKeySupport | UnitGenerateOptions.ParseMethod)]
public readonly partial struct PlayerScoreSnapshotId
{
    public PlayerScoreSnapshotId()
    {
        this.value = Ulid.Empty;
    }
}
