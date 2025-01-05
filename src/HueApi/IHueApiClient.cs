using HueApi.Models;

namespace HueApi
{
  public interface IHueApiClient
  {
    void Dispose();
    Task<HueResponse<ResourceIdentifier>> HueDeleteRequestAsync(string url);
    Task<HueResponse<T>> HueGetRequestAsync<T>(string url);
    Task<HueResponse<ResourceIdentifier>> HuePostRequestAsync<D>(string url, D data);
    Task<HueResponse<ResourceIdentifier>> HuePutRequestAsync<D>(string url, D data);
  }
}
