using System.Diagnostics;
using System.Text.Json.Serialization;

namespace HueApi.Models
{
  [DebuggerDisplay("{Type} | {IdV1} | {Id}")]
  public class MatterItem
  {
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = default!;

    [JsonPropertyName("id_v1")]
    public string? IdV1 { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("max_fabrics")]
    public int MaxFabrics { get; set; } = default!;

    [JsonPropertyName("has_qr_code")]
    public bool HasQrCode { get; set; }

  }
}
