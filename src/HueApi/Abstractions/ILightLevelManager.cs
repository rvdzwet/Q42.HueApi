using HueApi.Models;
using HueApi.Models.Sensors;

namespace HueApi.Abstractions
{
  public interface ILightLevelManager : IReadResourceManagement<LightLevel>
  { }

  public interface IDeviceManager : IReadResourceManagement<Device> { }
}
