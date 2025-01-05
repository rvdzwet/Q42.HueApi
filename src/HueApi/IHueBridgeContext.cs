namespace HueApi
{
  public interface IHueBridgeContext
  {
    void SetBridge(string ip, IBridgeInfo bridge);
    IBridgeInfo GetBridge(string ip);
    void RemoveBridge(string ip);
    bool ContainsBridge(string ip);
    IReadOnlyDictionary<string, IBridgeInfo> GetBridges();
  }
}
