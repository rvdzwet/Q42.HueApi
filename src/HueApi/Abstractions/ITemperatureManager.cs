using HueApi.Models.Sensors;

namespace HueApi.Abstractions
{
  public interface ITemperatureManager : IReadResourceManagement<TemperatureResource>
  { }
}
