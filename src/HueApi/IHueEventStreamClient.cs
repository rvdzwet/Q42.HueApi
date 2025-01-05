namespace HueApi
{
  /// <summary>
  /// Defines an interface for a client that handles communication with the Hue bridge's event stream.
  /// </summary>
  public interface IHueEventStreamClient
  {
    /// <summary>
    /// Occurs when a new message is received from the event stream.
    /// </summary>
    event EventStreamMessage? OnEventStreamMessage;

    /// <summary>
    /// Disposes of the client and releases any associated resources.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Starts listening for events from the Hue bridge event stream.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops listening for events from the Hue bridge event stream.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task StopAsync();
  }
}
