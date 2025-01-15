using System.Collections.Concurrent;

namespace HueApi
{

  public class HueBridgeContext : IHueBridgeContext
  {
    private readonly ConcurrentDictionary<string, IBridgeInfo> _bridges = new();

    public void SetBridge(string ip, IBridgeInfo bridge)
    {
      if (string.IsNullOrWhiteSpace(ip))
      {
        throw new ArgumentNullException(nameof(ip));
      }

      if (bridge is null)
      {
        throw new ArgumentNullException(nameof(bridge));
      }

      _bridges[ip] = bridge;
    }

    public IBridgeInfo GetBridge(string ip)
    {
      if (_bridges.TryGetValue(ip, out var bridge))
      {
        return bridge;
      }

      throw new InvalidOperationException($"No bridge with IP '{ip}' has been set in the current context.");
    }

    public void RemoveBridge(string ip)
    {
      _bridges.TryRemove(ip, out _);
    }

    public bool ContainsBridge(string ip)
    {
      return _bridges.ContainsKey(ip);
    }

    public IReadOnlyDictionary<string, IBridgeInfo> GetBridges()
    {
      return _bridges;
    }
  }
}
