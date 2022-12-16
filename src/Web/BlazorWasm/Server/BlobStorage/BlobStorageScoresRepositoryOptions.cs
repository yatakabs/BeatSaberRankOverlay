using System.Text;
using System.Text.Json;

namespace RankOverlay.Web.BlazorWasm.Server.BlobStorage;

public class BlobStorageOptions
{
    public required string BaseDirectory { get; set; }
    public required JsonSerializerOptions JsonSerializerOptions { get; set; }
    public Encoding? TextEncoding { get; set; }
    public TimeSpan? TimeZoneOffset { get; set; }
}
