using HueApi.Abstractions;
using HueApi.Models;
using HueApi.Models.Requests;
using HueApi.Models.Sensors;

namespace HueApi.Managers
{
  public interface IHueManagerFactory
  {
    ILightManager CreateLightManager(IBridgeInfo bridge);
    IButtonManager CreateButtonManager(IBridgeInfo bridge);
    IMotionManager CreateMotionManager(IBridgeInfo bridge);
    ILightLevelManager CreateLightLevelManager(IBridgeInfo bridge);
    ITemperatureManager CreateTemperatureManager(IBridgeInfo bridge);
    IDeviceManager CreateDeviceManager(IBridgeInfo bridge);
  }

  internal class HueManagerFactory : IHueManagerFactory
  {
    private readonly IHueClientFactory _clientFactory;

    public HueManagerFactory(IHueClientFactory clientFactory)
    {
      _clientFactory = clientFactory;
    }

    public ILightManager CreateLightManager(IBridgeInfo bridge)
    {
      return new LightManager(_clientFactory.CreateApiClient(bridge));
    }

    public IButtonManager CreateButtonManager(IBridgeInfo bridge)
    {
      return new ButtonManager(_clientFactory.CreateApiClient(bridge));
    }

    public IMotionManager CreateMotionManager(IBridgeInfo bridge)
    {
      return new MotionManager(_clientFactory.CreateApiClient(bridge));
    }

    public ILightLevelManager CreateLightLevelManager(IBridgeInfo bridge)
    {
      return new LightLevelManager(_clientFactory.CreateApiClient(bridge));
    }

    public ITemperatureManager CreateTemperatureManager(IBridgeInfo bridge)
    {
      return new TemperatureManager(_clientFactory.CreateApiClient(bridge));
    }

    public IDeviceManager CreateDeviceManager(IBridgeInfo bridge)
    {
      return new DeviceManager(_clientFactory.CreateApiClient(bridge));
    }
  }

  internal class DeviceManager : ResourceManager<Models.Device, object, object>, IDeviceManager
  {
    public DeviceManager(IHueApiClient api) : base(api) { }
  }

  internal class LightManager : ResourceManager<Models.Light, UpdateLight, object>, ILightManager
  {
    public LightManager(IHueApiClient api) : base(api) { }
  }

  internal class ButtonManager : ResourceManager<ButtonResource, object, object>, IButtonManager
  {
    public ButtonManager(IHueApiClient api) : base(api) { }
  }

  internal class MotionManager : ResourceManager<MotionResource, object, object>, IMotionManager
  {
    public MotionManager(IHueApiClient api) : base(api) { }
  }

  internal class LightLevelManager : ResourceManager<LightLevel, object, object>, ILightLevelManager
  {
    public LightLevelManager(IHueApiClient api) : base(api) { }
  }

  internal class TemperatureManager : ResourceManager<TemperatureResource, object, object>, ITemperatureManager
  {
    public TemperatureManager(IHueApiClient api) : base(api) { }
  }
}
