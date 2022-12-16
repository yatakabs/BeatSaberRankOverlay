using System.Text.Json;
using System.Text.Json.Serialization;

namespace RankOverlay.JsonConverters;

public class ConcreteClassConverter<TAbstract, TConcrete> : JsonConverter<TAbstract>
        where TConcrete : TAbstract
{
    public override TAbstract Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<TConcrete>(ref reader, options)!;
    }

    public override void Write(Utf8JsonWriter writer, TAbstract value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(TAbstract), options);
    }
}
