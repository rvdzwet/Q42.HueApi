using HueApi.Models;

namespace HueApi.Managers
{
  /// <summary>
  /// Provides a base class for managing Hue resources of a specific type.
  /// This class handles common CRUD (Create, Read, Update, Delete) operations.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource (e.g., Light, Scene, Room).</typeparam>
  /// <typeparam name="TUpdateModel">The type of the model used for updating the resource.</typeparam>
  /// <typeparam name="TCreateModel">The type of the model used for creating the resource.</typeparam>
  internal abstract class ResourceManager<TResource, TUpdateModel, TCreateModel> where TResource : HueResource
  {
    /// <summary>
    /// The Hue API client used to make requests.
    /// </summary>
    protected readonly IHueApiClient _api;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceManager{TResource, TUpdateModel, TCreateModel}"/> class.
    /// </summary>
    /// <param name="api">The Hue API client.</param>
    protected ResourceManager(IHueApiClient api)
    {
      _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    /// <summary>
    /// Retrieves a specific resource by its ID.
    /// </summary>
    /// <param name="id">The ID of the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{TResource}"/>.</returns>
    public virtual Task<HueResponse<TResource>> GetResourceAsync(Guid id)
    {
      return _api.HueGetRequestAsync<TResource>(ResourceUtils.ResourceIdUrl(ResourceUtils.GetResourceUrl<TResource>(), id));
    }

    /// <summary>
    /// Retrieves all resources of the specified type.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{TResource}"/>.</returns>
    public virtual Task<HueResponse<TResource>> GetResourcesAsync()
    {
      return _api.HueGetRequestAsync<TResource>(ResourceUtils.GetResourceUrl<TResource>());
    }

    /// <summary>
    /// Updates a specific resource.
    /// </summary>
    /// <param name="id">The ID of the resource to update.</param>
    /// <param name="data">The data used to update the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    public virtual Task<HueResponse<ResourceIdentifier>> UpdateResourceAsync(Guid id, TUpdateModel data)
    {
      return _api.HuePutRequestAsync(ResourceUtils.ResourceIdUrl(ResourceUtils.GetResourceUrl<TResource>(), id), data);
    }

    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="id">The id that will be used for creation</param>
    /// <param name="data">The data used to create the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    public virtual Task<HueResponse<ResourceIdentifier>> CreateResourceAsync(Guid id, TCreateModel data)
    {
      return _api.HuePostRequestAsync(ResourceUtils.ResourceIdUrl(ResourceUtils.GetResourceUrl<TResource>(), id), data);
    }

    /// <summary>
    /// Deletes a specific resource.
    /// </summary>
    /// <param name="id">The ID of the resource to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    public virtual Task<HueResponse<ResourceIdentifier>> DeleteResourceAsync(Guid id)
    {
      return _api.HueDeleteRequestAsync(ResourceUtils.ResourceIdUrl(ResourceUtils.GetResourceUrl<TResource>(), id));
    }
  }
}
