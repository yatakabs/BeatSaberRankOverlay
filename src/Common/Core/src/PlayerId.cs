using UnitGenerator;

namespace RankOverlay;

[UnitOf(typeof(string), UnitGenerateOptions.ImplicitOperator | UnitGenerateOptions.JsonConverter | UnitGenerateOptions.JsonConverterDictionaryKeySupport)]
public readonly partial struct PlayerId
{
    public PlayerId()
    {
        this.value = string.Empty;
    }

    public static PlayerId Empty { get; } = new PlayerId();
    public static PlayerId New()
    {
        return Ulid.NewUlid().ToString().ToLower();
    }
}
