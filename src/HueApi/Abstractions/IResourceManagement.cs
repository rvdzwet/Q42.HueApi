using HueApi.Models;

namespace HueApi.Abstractions
{
  /// <summary>
  /// Defines an interface for managing read operations on Hue resources.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource.</typeparam>
  public interface IReadResourceManagement<TResource> where TResource : HueResource
  {
    /// <summary>
    /// Retrieves all resources of the specified type.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{TResource}"/>.</returns>
    Task<HueResponse<TResource>> GetResourcesAsync();

    /// <summary>
    /// Retrieves a specific resource by its ID.
    /// </summary>
    /// <param name="id">The ID of the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{TResource}"/>.</returns>
    Task<HueResponse<TResource>> GetResourceAsync(Guid id);
  }

  /// <summary>
  /// Defines an interface for managing update operations on Hue resources.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource.</typeparam>
  /// <typeparam name="TUpdateModel">The type of the model used for updating the resource.</typeparam>
  public interface IUpdateResourceManagement<TResource, TUpdateModel> where TResource : HueResource
  {
    /// <summary>
    /// Updates a specific resource.
    /// </summary>
    /// <param name="id">The ID of the resource to update.</param>
    /// <param name="data">The data used to update the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    Task<HueResponse<ResourceIdentifier>> UpdateResourceAsync(Guid id, TUpdateModel data);
  }

  /// <summary>
  /// Defines an interface for managing delete operations on Hue resources.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource.</typeparam>
  public interface IDeleteResourceManagement<TResource> where TResource : HueResource
  {
    /// <summary>
    /// Deletes a specific resource.
    /// </summary>
    /// <param name="id">The ID of the resource to delete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    Task<HueResponse<ResourceIdentifier>> DeleteResourceAsync(Guid id);
  }

  /// <summary>
  /// Defines an interface for managing create operations on Hue resources.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource.</typeparam>
  /// <typeparam name="TCreateModel">The type of the model used for creating the resource.</typeparam>
  public interface ICreateResourceManagement<TResource, TCreateModel> where TResource : HueResource
  {
    /// <summary>
    /// Creates a new resource.
    /// </summary>
    /// <param name="resource">The data used to create the resource.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{ResourceIdentifier}"/>.</returns>
    Task<HueResponse<ResourceIdentifier>> CreateResourceAsync(TCreateModel resource);
  }

  /// <summary>
  /// Defines a comprehensive interface for managing Hue resources, combining read, update, create and delete operations.
  /// </summary>
  /// <typeparam name="TResource">The type of the Hue resource.</typeparam>
  /// <typeparam name="TUpdateModel">The type of the model used for updating the resource.</typeparam>
  /// <typeparam name="TCreateModel">The type of the model used for creating the resource.</typeparam>
  public interface IResourceManagement<TResource, TUpdateModel, TCreateModel> :
      IReadResourceManagement<TResource>,
      IUpdateResourceManagement<TResource, TUpdateModel>,
      IDeleteResourceManagement<TResource>,
      ICreateResourceManagement<TResource, TCreateModel>
      where TResource : HueResource
  {
  }
}
