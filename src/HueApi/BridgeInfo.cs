using System.Net;

namespace HueApi
{
  /// <summary>
  /// Represents a discovered Hue bridge.
  /// </summary>
  public class BridgeInfo : IBridgeInfo
  {
    /// <inheritdoc />
    public IPAddress Ip { get; set; } = IPAddress.None; // Initialize to avoid null exceptions

    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Key { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    public override string ToString()
    {
      return $"Name: {Name}, IP: {Ip}, Key: {Key}";
    }
  }
}
