using Microsoft.AspNetCore.Components;

namespace Plan2Gather.Services;

/// <summary>
/// Provides a pre-configured HttpClient pointing at the current server's base address.
/// Inject this (or IHttpClientFactory) in your Blazor components.
/// </summary>
public class ApiHttpClientFactory(IHttpClientFactory factory, NavigationManager nav)
{
    public HttpClient Create()
    {
        var client = factory.CreateClient("API");
        // NavigationManager.BaseUri gives the current server's root (e.g. https://localhost:5001/)
        client.BaseAddress = new Uri(nav.BaseUri);
        return client;
    }
}
