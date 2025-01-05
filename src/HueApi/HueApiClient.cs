using HueApi.Models;
using HueApi.Models.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HueApi
{

  

  public class InvalidBridgeConfigurationException : Exception
  {
    public InvalidBridgeConfigurationException(string message) : base(message) { }
    public InvalidBridgeConfigurationException(string message, Exception innerException) : base(message, innerException) { }
  }

  public class NoBridgesFoundException : Exception
  {
    public NoBridgesFoundException() : base("No bridges found in the context. Please discover and add a bridge first.") { }
    public NoBridgesFoundException(string message) : base(message) { }
    public NoBridgesFoundException(string message, Exception innerException) : base(message, innerException) { }
  }

  public class MultipleBridgesFoundException : Exception
  {
    public MultipleBridgesFoundException() : base("Multiple bridges found in the context. Please specify which bridge to use.") { }
    public MultipleBridgesFoundException(string message) : base(message) { }
    public MultipleBridgesFoundException(string message, Exception innerException) : base(message, innerException) { }
  }


  public interface IHueClientFactory
  {
    IHueApiClient CreateApiClient(string ip);
    IHueApiClient CreateApiClient(IBridgeInfo bridge);
    IHueApiClient CreateDefaultApiClient();
    IHueEventStreamClient CreateEventStreamClient(string ip);
    IHueEventStreamClient CreateEventStreamClient(IBridgeInfo bridge);
    IHueEventStreamClient CreateDefaultEventStreamClient();
  }

  internal class HueClientFactory : IHueClientFactory, IDisposable
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HueApiClient> _apiClientLogger;
    private readonly ILogger<HueEventStreamClient> _eventStreamLogger;
    private readonly IHueBridgeContext _bridgeContext;
    private readonly ConcurrentDictionary<string, IHueApiClient> _apiClients = new();
    private readonly ConcurrentDictionary<string, IHueEventStreamClient> _eventStreamClients = new();
    private bool _disposed = false;

    public HueClientFactory(IHttpClientFactory httpClientFactory, ILogger<HueApiClient> apiClientLogger, ILogger<HueEventStreamClient> eventStreamLogger, IHueBridgeContext bridgeContext)
    {
      _httpClientFactory = httpClientFactory;
      _apiClientLogger = apiClientLogger;
      _eventStreamLogger = eventStreamLogger;
      _bridgeContext = bridgeContext;
    }

    public IHueApiClient CreateApiClient(string ip)
    {
      _apiClientLogger.LogDebug("Creating or retrieving ApiClient for IP: {Ip}", ip);

      return _apiClients.GetOrAdd(ip, _ =>
      {
        _apiClientLogger.LogInformation("Creating new ApiClient for IP: {Ip}", ip);
        try
        {
          var bridge = _bridgeContext.GetBridge(ip);
          return new HueApiClient(_apiClientLogger, _httpClientFactory, bridge);
        }
        catch (InvalidOperationException ex)
        {
          _apiClientLogger.LogError(ex, "Failed to create ApiClient for IP: {Ip}. Bridge not found.", ip);
          throw; // Re-throw the exception after logging
        }

      });
    }

    public IHueApiClient CreateApiClient(IBridgeInfo bridge)
    {
      _apiClientLogger.LogDebug("Creating or retrieving ApiClient for Bridge: {BridgeName} ({BridgeIp})", bridge.Name, bridge.Ip);
      return CreateApiClient(bridge.Ip.ToString());
    }

    public IHueApiClient CreateDefaultApiClient()
    {
      _apiClientLogger.LogDebug("Creating default ApiClient");
      IReadOnlyDictionary<string, IBridgeInfo> bridges = _bridgeContext.GetBridges();
      if (!bridges.Any())
      {
        _apiClientLogger.LogError("No bridges found. Unable to create default ApiClient.");
        throw new NoBridgesFoundException();
      }
      if (bridges.Count > 1)
      {
        _apiClientLogger.LogError("Multiple bridges found. Unable to create default ApiClient. Please specify a bridge.");
        throw new MultipleBridgesFoundException();
      }
      _apiClientLogger.LogInformation("Creating default ApiClient for bridge: {BridgeName} ({BridgeIp})", bridges.First().Value.Name, bridges.First().Value.Ip);
      return CreateApiClient(bridges.First().Value);
    }

    public IHueEventStreamClient CreateEventStreamClient(IBridgeInfo bridge)
    {
      _eventStreamLogger.LogDebug("Creating or retrieving EventStreamClient for Bridge: {BridgeName} ({BridgeIp})", bridge.Name, bridge.Ip);
      return _eventStreamClients.GetOrAdd(bridge.Ip.ToString(), _ =>
      {
        _eventStreamLogger.LogInformation("Creating new EventStreamClient for Bridge: {BridgeName} ({BridgeIp})", bridge.Name, bridge.Ip);
        return new HueEventStreamClient(_eventStreamLogger, _httpClientFactory, bridge);
      });
    }

    public IHueEventStreamClient CreateEventStreamClient(string ip)
    {
      _eventStreamLogger.LogDebug("Creating or retrieving EventStreamClient for IP: {Ip}", ip);
      try
      {
        var bridge = _bridgeContext.GetBridge(ip);
        return CreateEventStreamClient(bridge);
      }
      catch (InvalidOperationException ex)
      {
        _eventStreamLogger.LogError(ex, "Failed to create EventStreamClient for IP: {Ip}. Bridge not found.", ip);
        throw;
      }

    }

    public IHueEventStreamClient CreateDefaultEventStreamClient()
    {
      _eventStreamLogger.LogDebug("Creating default EventStreamClient");
      IReadOnlyDictionary<string, IBridgeInfo> bridges = _bridgeContext.GetBridges();
      if (!bridges.Any())
      {
        _eventStreamLogger.LogError("No bridges found. Unable to create default EventStreamClient.");
        throw new NoBridgesFoundException();
      }
      if (bridges.Count > 1)
      {
        _eventStreamLogger.LogError("Multiple bridges found. Unable to create default EventStreamClient. Please specify a bridge.");
        throw new MultipleBridgesFoundException();
      }
      _eventStreamLogger.LogInformation("Creating default EventStreamClient for bridge: {BridgeName} ({BridgeIp})", bridges.First().Value.Name, bridges.First().Value.Ip);
      return CreateEventStreamClient(bridges.First().Value);
    }

    public void Dispose()
    {
      if (_disposed)
      {
        return;
      }

      foreach (var client in _apiClients.Values)
      {
        (client as IDisposable)?.Dispose();
      }
      _apiClients.Clear();

      foreach (var client in _eventStreamClients.Values)
      {
        (client as IDisposable)?.Dispose();
      }
      _eventStreamClients.Clear();

      _disposed = true;
    }
  }

  /// <summary>
  /// Abstract base class for Hue API clients. Provides common functionality for making HTTP requests
  /// and processing responses. This class is thread-safe due to the use of IHttpClientFactory.
  /// </summary>
  internal class HueApiClient : IDisposable, IHueApiClient
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HueApiClient> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private IBridgeInfo _bridge;

    /// <summary>
    /// Initializes a new instance of the <see cref="HueApiClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="httpClientFactory">The HTTP client factory used to create HTTP clients.</param>
    /// <exception cref="ArgumentNullException">Thrown if either <paramref name="logger"/> or <paramref name="httpClientFactory"/> is null.</exception>
    public HueApiClient(ILogger<HueApiClient> logger, IHttpClientFactory httpClientFactory, IBridgeInfo bridge)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));

      _logger = logger;
      _httpClientFactory = httpClientFactory;
      _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
      _jsonSerializerOptions = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };
    }

    /// <inheritdoc />
    public void Dispose()
    {
      // No resources to dispose in this example, but keep the method for future use.
    }

    private void ValidateBridge()
    {
      if (_bridge.Ip == null)
      {
        throw new InvalidBridgeConfigurationException("Bridge IP is not set.");
      }

      if (string.IsNullOrEmpty(_bridge.Key)) // Use IsNullOrEmpty for string check
      {
        throw new InvalidBridgeConfigurationException("Bridge Key is not set.");
      }
    }

    /// <summary>
    /// Sends an HTTP request asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the request body (if any).</typeparam>
    /// <param name="method">The HTTP method to use (e.g., GET, POST, PUT, DELETE).</param>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="data">The request body (optional).</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HttpResponseMessage"/>.</returns>
    protected async Task<HttpResponseMessage> SendRequestAsync<T>(HttpMethod method, string url, T? data = default)
    {
      ValidateBridge();

      using var client = _httpClientFactory.CreateClient(HueConstants.HUE_API_CLIENT_NAME);
      client.BaseAddress = new Uri($"https://{_bridge.Ip}/");

      _logger.LogDebug("{method} request to: {url}{dataLog}", method, url, data != null ? $" with data: {JsonSerializer.Serialize(data)}" : "");

      HttpRequestMessage request = new HttpRequestMessage(method, url);
      request.Headers.Add(HueConstants.APPLICATION_KEY_HEADER, _bridge.Key);

      if (data != null)
      {
        request.Content = JsonContent.Create(data, options: _jsonSerializerOptions);
      }

      return await client.SendAsync(request);
    }

    /// <summary>
    /// Sends an HTTP DELETE request asynchronously.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueDeleteResponse"/>.</returns>
    public async Task<HueResponse<ResourceIdentifier>> HueDeleteRequestAsync(string url)
    {
      var response = await SendRequestAsync<object>(HttpMethod.Delete, url);
      return await ProcessResponseAsync<HueDeleteResponse>(response, HttpMethod.Delete, url);
    }

    /// <summary>
    /// Sends an HTTP GET request asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the expected response body.</typeparam>
    /// <param name="url">The URL to send the request to.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HueResponse{T}"/>.</returns>
    public async Task<HueResponse<T>> HueGetRequestAsync<T>(string url)
    {
      var response = await SendRequestAsync<object>(HttpMethod.Get, url);
      return await ProcessResponseAsync<HueResponse<T>>(response, HttpMethod.Get, url);
    }

    /// <summary>
    /// Sends an HTTP POST request asynchronously.
    /// </summary>
    /// <typeparam name="D">The type of the request body.</typeparam>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="data">The request body.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HuePostResponse"/>.</returns>
    public async Task<HueResponse<ResourceIdentifier>> HuePostRequestAsync<D>(string url, D data)
    {
      var response = await SendRequestAsync(HttpMethod.Post, url, data);
      return await ProcessResponseAsync<HuePostResponse>(response, HttpMethod.Post, url);
    }

    /// <summary>
    /// Sends an HTTP PUT request asynchronously.
    /// </summary>
    /// <typeparam name="D">The type of the request body.</typeparam>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="data">The request body.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the <see cref="HuePutResponse"/>.</returns>
    public async Task<HueResponse<ResourceIdentifier>> HuePutRequestAsync<D>(string url, D data)
    {
      var response = await SendRequestAsync(HttpMethod.Put, url, data);
      return await ProcessResponseAsync<HuePutResponse>(response, HttpMethod.Put, url);
    }

    /// <summary>
    /// Processes the HTTP response and returns the deserialized response object.
    /// </summary>
    /// <typeparam name="T">The type of the expected response object.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="method">The HTTP method used for the request.</param>
    /// <param name="url">The URL used for the request.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the deserialized response object.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the response status code is 403 Forbidden.</exception>
    protected async Task<T> ProcessResponseAsync<T>(HttpResponseMessage? response, HttpMethod method, string url) where T : HueErrorResponse, new()
    {
      if (response == null)
      {
        _logger.LogError("Received null HTTP response for {method} {url}.", method, url);
        return new T();
      }

      _logger.LogInformation("Received HTTP response {statusCode} for {method} {url}.", response.StatusCode, method, url);

      if (response.IsSuccessStatusCode)
      {
        try
        {
          return (await response.Content.ReadFromJsonAsync<T>()) ?? new();
        }
        catch (JsonException ex)
        {
          _logger.LogError(ex, "Failed to deserialize JSON response from {method} {url}: {content}", method, url, await response.Content.ReadAsStringAsync());
          throw;
        }
      }
      else if (response.StatusCode == HttpStatusCode.Forbidden)
      {
        _logger.LogWarning("Unauthorized access attempt for {method} {url}: {statusCode}", method, url, response.StatusCode);
        throw new UnauthorizedAccessException();
      }
      else
      {
        string? errorContent = null;
        try
        {
          errorContent = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to read error content from {method} {url}.", method, url);
        }

        _logger.LogError("API request failed for {method} {url}: {statusCode} - {errorContent}", method, url, response.StatusCode, errorContent);

        T errorResponse = new T(); // Create a new instance here to guarantee a non-null return value.
        try
        {
          var deserializedErrorResponse = await response.Content.ReadFromJsonAsync<T>();
          if (deserializedErrorResponse != null)
          {
            errorResponse = deserializedErrorResponse;
          }
          else
          {
            _logger.LogWarning("Failed to deserialize error response content from {method} {url}. Returning empty error response.", method, url);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to deserialize error response from {method} {url}.", method, url);
        }

        if (errorResponse != null)
          errorResponse.Errors = errorResponse.Errors; // This line is now safe

        return errorResponse ?? new T();
      }
    }
  }
}
