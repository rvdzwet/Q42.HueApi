using HueApi.Abstractions;
using HueApi.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Security;

namespace HueApi
{
  /// <summary>
  /// Provides extension methods for registering Hue API services with an <see cref="IServiceCollection"/>.
  /// </summary>
  public static class HueApiServiceCollectionExtensions
  {
    /// <summary>
    /// Adds the local Hue API services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">An <see cref="Action{HueApiOptions}"/> to configure the Hue API options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configureOptions"/> is null.</exception>
    public static IServiceCollection AddLocalHueApi(this IServiceCollection services)
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));
      
      services.AddHttpClient(HueConstants.HUE_API_CLIENT_NAME)
      .ConfigurePrimaryHttpMessageHandler(() =>
      {
        // Allowing Untrusted SSL Certificates (Use with caution in production)
        var handler = new HttpClientHandler
        {
          UseCookies = false,
          MaxConnectionsPerServer = 1, // Important for avoiding port exhaustion
          ClientCertificateOptions = ClientCertificateOption.Manual,
          ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };

        return handler;
      });

      services.AddHttpClient(HueConstants.HUE_EVENT_STREAM_CLIENT_NAME, (serviceProvider, client) =>
      {
        client.Timeout = Timeout.InfiniteTimeSpan;
      }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
      {
        UseCookies = false,
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        UseProxy = false,
        PooledConnectionLifetime = TimeSpan.MaxValue,
        PooledConnectionIdleTimeout = TimeSpan.FromSeconds(60),
        MaxConnectionsPerServer = 1,
        SslOptions = new SslClientAuthenticationOptions // Correct way to set the callback
        {
          RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
          {
            return true; // Accept all certificates
          }
        }
      });

      services.AddScoped<IHueBridgeDiscoveryService, HueBridgeZeroConfDiscoveryService>();
      services.AddScoped<IHueBridgeContext, HueBridgeContext>();
      services.AddSingleton<IHueClientFactory, HueClientFactory>();
      services.AddSingleton<IHueManagerFactory, HueManagerFactory>();
      // ...Register other managers

      return services;
    }
  }
}
