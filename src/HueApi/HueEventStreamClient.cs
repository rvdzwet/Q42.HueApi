using HueApi.Models.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HueApi
{
  /// <summary>
  /// Represents a delegate for handling messages received from the Hue bridge event stream.
  /// </summary>
  /// <param name="events">A list of <see cref="EventStreamResponse"/> objects received in the message.</param>
  public delegate void EventStreamMessage(List<EventStreamResponse> events);

  internal class HueEventStreamClient : IHueEventStreamClient
  {
    private readonly ILogger<HueEventStreamClient> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private CancellationTokenSource? _eventStreamCancellationTokenSource;
    private readonly IBridgeInfo _bridge;
    private bool _isRunning;

    /// <inheritdoc/>
    public event EventStreamMessage? OnEventStreamMessage;

    public HueEventStreamClient(ILogger<HueEventStreamClient> logger, IHttpClientFactory httpClientFactory, IBridgeInfo bridge)
    {
      ArgumentNullException.ThrowIfNull(logger, nameof(logger));
      ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));

      _logger = logger;
      _httpClientFactory = httpClientFactory;
      _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
      _jsonSerializerOptions = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true // Important for Hue API!
      };
    }

    /// <inheritdoc/>
    public async Task StartAsync()
    {
      if (_isRunning)
      {
        _logger.LogInformation("Hue Event Stream already running.");
        return;
      }

      _logger.LogInformation("Starting Hue Event Stream...");

      ValidateBridge();

      _eventStreamCancellationTokenSource?.Cancel();
      _eventStreamCancellationTokenSource = new CancellationTokenSource();
      var cancelToken = _eventStreamCancellationTokenSource.Token;

      _isRunning = true;

      var success = false;
      while (!cancelToken.IsCancellationRequested)
      {
        success = await AttemptEventStreamConnectionAsync(cancelToken);
        if (!success)
        {
          await Task.Delay(TimeSpan.FromSeconds(10), cancelToken);
          //TODO //dispatch event that the stream has disconnected. also dispatch when the stream has connected again so i can refresh all devices
        }
      }

      _isRunning = false;
    }

    private async Task<bool> AttemptEventStreamConnectionAsync(CancellationToken cancelToken)
    {

      try
      {
        using var client = _httpClientFactory.CreateClient(HueConstants.HUE_EVENT_STREAM_CLIENT_NAME);
        client.BaseAddress = new Uri($"https://{_bridge.Ip}/");
        client.DefaultRequestHeaders.Add(HueConstants.APPLICATION_KEY_HEADER, _bridge.Key);

        using var stream = await client.GetStreamAsync(HueConstants.HUE_EVENT_STREAM_URL, cancelToken);
        using var streamReader = new StreamReader(stream, encoding: Encoding.UTF8);

        await ProcessStreamMessagesAsync(streamReader, cancelToken);

        return true;
      }
      catch (OperationCanceledException)
      {
        if (cancelToken.IsCancellationRequested)
        {
          _logger.LogInformation("Event Stream Stopped.");
          return false;
        }
        else
        {
          _logger.LogWarning("Event Stream Connection Canceled. Reconnecting...");
          return false;
        }
      }
      catch (InvalidOperationException ex)
      {
        _logger.LogWarning("Invalid Operation Exception in Event Stream: {0}", ex.Message);
        return true;
      }
      catch (HttpRequestException ex)
      {
        _logger.LogError(ex, "HTTP Request Exception in Event Stream.");
        return false;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unhandled exception in Event Stream.");
        return false;
      }
    }

    private async Task ProcessStreamMessagesAsync(StreamReader streamReader, CancellationToken cancelToken)
    {
      while (!streamReader.EndOfStream && !cancelToken.IsCancellationRequested)
      {
        try
        {
          var jsonMsg = await streamReader.ReadLineAsync(cancelToken);

          if (!string.IsNullOrWhiteSpace(jsonMsg))
          {
            ProcessEventMessage(jsonMsg, cancelToken);
          }
        }
        catch (OperationCanceledException)
        {
          break;
        }
        catch (IOException ex)
        {
          _logger.LogError(ex, "IOException while reading from stream.");
          break;
        }
      }
    }

    private void ProcessEventMessage(string jsonMsg, CancellationToken cancelToken)
    {
      try
      {
        var data = JsonSerializer.Deserialize<List<EventStreamResponse>>(jsonMsg, _jsonSerializerOptions);
        if (data != null && data.Any())
        {
          OnEventStreamMessage?.Invoke(data);
        }
      }
      catch (JsonException ex)
      {
        _logger.LogError(ex, "Error deserializing event stream message: {JsonMsg}", jsonMsg);
      }
    }

    /// <inheritdoc/>
    public Task StopAsync()
    {
      _logger.LogInformation("Stopping Hue Event Stream...");
      _eventStreamCancellationTokenSource?.Cancel();
      return Task.CompletedTask;
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

    /// <inheritdoc/>
    public void Dispose()
    {
      _eventStreamCancellationTokenSource?.Dispose();
    }
  }
}
