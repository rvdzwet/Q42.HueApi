using HueApi.Models.Requests;
using Light = HueApi.Models.Light;

namespace HueApi.Abstractions
{
  public interface ILightManager: IReadResourceManagement<Light>, IUpdateResourceManagement<Light, UpdateLight>
  { }
}
