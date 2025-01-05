using HueApi.Models;
using HueApi.Models.Sensors;
using System.Reflection;

namespace HueApi
{
  public class HueResourceNameAttribute : Attribute
  {
    public Type ResourceType { get; }

    public HueResourceNameAttribute(Type resourceType)
    {
      ResourceType = resourceType;
    }
  }

  internal static class ResourceUtils
  {
    public const string ResourceUrl = "clip/v2/resource";

    [HueResourceName(typeof(Models.Light))]
    public const string LightUrl = $"{ResourceUrl}/light";
    [HueResourceName(typeof(Scene))]
    public const string SceneUrl = $"{ResourceUrl}/scene";
    [HueResourceName(typeof(Room))]
    public const string RoomUrl = $"{ResourceUrl}/room";
    [HueResourceName(typeof(Zone))]
    public const string ZoneUrl = $"{ResourceUrl}/zone";
    [HueResourceName(typeof(BridgeHome))]
    public const string BridgeHomeUrl = $"{ResourceUrl}/bridge_home";
    [HueResourceName(typeof(GroupedLight))]
    public const string GroupedLightUrl = $"{ResourceUrl}/grouped_light";
    [HueResourceName(typeof(Device))]
    public const string DeviceUrl = $"{ResourceUrl}/device";
    [HueResourceName(typeof(Bridge))]
    public const string BridgeUrl = $"{ResourceUrl}/bridge";
    [HueResourceName(typeof(DevicePower))]
    public const string DevicePowerUrl = $"{ResourceUrl}/device_power";
    [HueResourceName(typeof(MotionResource))]
    public const string MotionUrl = $"{ResourceUrl}/motion";
    [HueResourceName(typeof(CameraMotionResource))]
    public const string CameraMotionUrl = $"{ResourceUrl}/camera_motion";
    [HueResourceName(typeof(TemperatureResource))]
    public const string TemperatureUrl = $"{ResourceUrl}/temperature";
    [HueResourceName(typeof(LightLevel))]
    public const string LightLevelUrl = $"{ResourceUrl}/light_level";
    [HueResourceName(typeof(ButtonResource))]
    public const string ButtonUrl = $"{ResourceUrl}/button";
    [HueResourceName(typeof(RelativeRotaryResource))]
    public const string RelativeRotaryUrl = $"{ResourceUrl}/relative_rotary";
    [HueResourceName(typeof(BehaviorScript))]
    public const string BehaviorScriptUrl = $"{ResourceUrl}/behavior_script";
    [HueResourceName(typeof(BehaviorInstance))]
    public const string BehaviorInstanceUrl = $"{ResourceUrl}/behavior_instance";
    [HueResourceName(typeof(GeofenceClient))]
    public const string GeofenceClientUrl = $"{ResourceUrl}/geofence_client";
    [HueResourceName(typeof(Geolocation))]
    public const string GeolocationUrl = $"{ResourceUrl}/geolocation";
    [HueResourceName(typeof(EntertainmentConfiguration))]
    public const string EntertainmentConfigurationUrl = $"{ResourceUrl}/entertainment_configuration";
    [HueResourceName(typeof(Entertainment))]
    public const string EntertainmentUrl = $"{ResourceUrl}/entertainment";
    [HueResourceName(typeof(Homekit))]
    public const string HomekitUrl = $"{ResourceUrl}/homekit";
    [HueResourceName(typeof(SmartScene))]
    public const string SmartSceneUrl = $"{ResourceUrl}/smart_scene";
    [HueResourceName(typeof(ContactSensor))]
    public const string ContactUrl = $"{ResourceUrl}/contact";
    [HueResourceName(typeof(TamperSensor))]
    public const string TamperUrl = $"{ResourceUrl}/tamper";

    public static string ResourceIdUrl(string resourceUrl, Guid id) => $"{resourceUrl}/{id}";

    public static string GetResourceUrl<T>() where T : HueResource
    {
      var type = typeof(T);
      var fields = typeof(ResourceUtils).GetFields(BindingFlags.Public | BindingFlags.Static);

      foreach (var field in fields)
      {
        var attribute = field.GetCustomAttribute<HueResourceNameAttribute>();
        if (attribute != null && attribute.ResourceType == type)
        {
          if (field.GetValue(null) is string retVal)
            return retVal;
        }
      }

      throw new ArgumentException($"No URL found for resource type: {type.Name}");
    }
  }
}
