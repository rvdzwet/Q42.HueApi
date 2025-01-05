using System.Net;

namespace HueApi
{
  /// <summary>
  /// Represents information about a discovered Hue bridge.
  /// </summary>
  public interface IBridgeInfo // Or public interface HueBridge
  {
    /// <summary>
    /// The ID of the Hue bridge.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The IP address of the Hue bridge.
    /// </summary>
    IPAddress Ip { get; }

    /// <summary>
    /// The friendly name of the Hue bridge.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The API key associated with the Hue bridge.
    /// </summary>
    string Key { internal get; set; }
  }
}
