namespace HueApi
{
  /// <summary>
  /// Contains constants used by the Hue API client.
  /// </summary>
  internal static class HueConstants
  {
    /// <summary>
    /// The name of the HttpClient used for Hue API requests.
    /// </summary>
    public const string HUE_API_CLIENT_NAME = "HueApiClient";

    /// <summary>
    /// The name of the HttpClient used for Hue API requests.
    /// </summary>
    public const string HUE_EVENT_STREAM_CLIENT_NAME = "HueEventStreamClient";

    /// <summary>
    /// The resource location of the Hue event stream.
    /// </summary>
    public const string HUE_EVENT_STREAM_URL = "eventstream/clip/v2";

    /// <summary>
    /// The name of the header containing the application key.
    /// </summary>
    public const string APPLICATION_KEY_HEADER = "hue-application-key";

    /// <summary>
    /// The name of the service used by mDNS to discover hue bridges.
    /// </summary>
    public const string MULTICAST_SERVICE_NAME = "_hue._tcp.local.";
  }
}
