using System.Net;
using Microsoft.Extensions.Logging;
using Zeroconf;

namespace HueApi
{
  public interface IHueBridgeDiscoveryService
  {
    Task<IEnumerable<IBridgeInfo>> DiscoverBridgesAsync();
  }

  internal class HueBridgeZeroConfDiscoveryService : IHueBridgeDiscoveryService
  {
    private readonly ILogger<HueBridgeZeroConfDiscoveryService> _logger;

    public HueBridgeZeroConfDiscoveryService(ILogger<HueBridgeZeroConfDiscoveryService> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<IBridgeInfo>> DiscoverBridgesAsync()
    {
      _logger.LogInformation("Starting Hue device discovery.");

      try
      {
        IReadOnlyList<IZeroconfHost> results = await ZeroconfResolver.ResolveAsync(HueConstants.MULTICAST_SERVICE_NAME);
        _logger.LogInformation($"Found {results.Count} potential Hue devices.");

        var devices = new List<IBridgeInfo>();
        foreach (var device in results)
        {
          _logger.LogInformation($"Found Hue device: {device.DisplayName} ({device.IPAddress})");
          devices.Add(new BridgeInfo()
          {
            Ip = IPAddress.Parse(device.IPAddress),
            Name = device.DisplayName,
            Id = device.Id
          });

        }

        return devices;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error during Hue device discovery.");
        return Enumerable.Empty<IBridgeInfo>();
      }
    }
  }
}
