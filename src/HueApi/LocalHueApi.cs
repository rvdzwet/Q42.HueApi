using HueApi.Models.Clip;
using HueApi.Models.Exceptions;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace HueApi
{

  public class LocalHueApi
  {
    /// <summary>
    /// Register your <paramref name="applicationName"/> and <paramref name="deviceName"/> at the Hue Bridge.
    /// </summary>
    /// <param name="ip">ip address of bridge</param>
    /// <param name="applicationName">The name of your app.</param>
    /// <param name="deviceName">The name of the device.</param>
    /// <param name="generateClientKey">Set to true if you want a client key to use the streaming api</param>
    /// <returns>Secret key for the app to communicate with the bridge.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="applicationName"/> or <paramref name="deviceName"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="applicationName"/> or <paramref name="deviceName"/> aren't long enough, are empty or contains spaces.</exception>
    public static async Task<RegisterEntertainmentResult?> RegisterAsync(string ip, string applicationName, string deviceName, bool generateClientKey = false, CancellationToken? cancellationToken = null)
    {
      if (!cancellationToken.HasValue)
        cancellationToken = new CancellationTokenSource().Token;

      if (applicationName == null)
        throw new ArgumentNullException(nameof(applicationName));
      if (applicationName.Trim() == String.Empty)
        throw new ArgumentException("applicationName must not be empty", nameof(applicationName));
      if (applicationName.Length > 20)
        throw new ArgumentException("applicationName max is 20 characters.", nameof(applicationName));
      if (applicationName.Contains(" "))
        throw new ArgumentException("Cannot contain spaces.", nameof(applicationName));

      if (deviceName == null)
        throw new ArgumentNullException(nameof(deviceName));
      if (deviceName.Length < 0 || deviceName.Trim() == String.Empty)
        throw new ArgumentException("deviceName must be at least 0 characters.", nameof(deviceName));
      if (deviceName.Length > 19)
        throw new ArgumentException("deviceName max is 19 characters.", nameof(deviceName));
      if (deviceName.Contains(" "))
        throw new ArgumentException("Cannot contain spaces.", nameof(deviceName));

      string fullName = string.Format("{0}#{1}", applicationName, deviceName);

      JsonObject obj = new JsonObject();
      obj["devicetype"] = fullName;

      if (generateClientKey)
        obj["generateclientkey"] = true;

      HttpClient client = new HttpClient();
      var response = await client.PostAsJsonAsync(new Uri($"http://{ip}/api"), obj, cancellationToken.Value).ConfigureAwait(false);
      var stringResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      JsonObject? result;
      try
      {
        var arrayResult = JsonNode.Parse(stringResponse)?.AsArray();
        result = arrayResult?.FirstOrDefault()?.AsObject();
      }
      catch
      {
        //Not an expected response. Return response as exception
        throw new HueException(stringResponse);
      }

      if (result != null)
      {
        JsonNode? error;
        if (result.TryGetPropertyValue("error", out error))
        {
          if (error?["type"]?.GetValue<int>() == 101) // link button not pressed
            throw new LinkButtonNotPressedException("Link button not pressed");
          else
            throw new HueException(error?["description"]?.GetValue<string>());
        }

        var username = result["success"]?["username"]?.GetValue<string>();
        var streamingClientKey = result["success"]?["clientkey"]?.GetValue<string>();

        if (username != null)
        {
          return new RegisterEntertainmentResult()
          {
            Ip = ip,
            Username = username,
            StreamingClientKey = streamingClientKey
          };
        }
      }

      return null;
    }
  }
}
