using HueApi.Models.Requests.Interface;
using System.Text.Json.Serialization;

namespace HueApi.Models
{
  public class LightAction : IUpdateColor, IUpdateColorTemperature, IUpdateOn, IUpdateDynamics
  {
    [JsonPropertyName("on")]
    public On? On { get; set; }

    [JsonPropertyName("dimming")]
    public Dimming? Dimming { get; set; }

    [JsonPropertyName("color")]
    public Color? Color { get; set; }

    [JsonPropertyName("color_temperature")]
    public ColorTemperature? ColorTemperature { get; set; }

    [JsonPropertyName("gradient")]
    public Gradient? Gradient { get; set; }

    [JsonPropertyName("effects")]
    public Effects? Effects { get; set; }

    [JsonPropertyName("dynamics")]
    public Dynamics? Dynamics { get; set; }
  }
}
