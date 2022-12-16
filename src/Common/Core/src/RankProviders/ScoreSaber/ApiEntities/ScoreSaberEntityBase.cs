using System.Text.Json;
using System.Text.Json.Serialization;

namespace RankOverlay.RankProviders.ScoreSaber.ApiEntities;

public record ScoreSaberEntityBase
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; init; } = new Dictionary<string, JsonElement>();
}
