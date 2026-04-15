using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Plan2Gather.Services;

namespace Plan2Gather.Auth;

/// <summary>
/// Reads the JWT stored in localStorage and exposes the ClaimsPrincipal to Blazor's auth system.
/// </summary>
public class JwtAuthStateProvider(IJSRuntime js, JwtService jwtService) : AuthenticationStateProvider
{
    private const string TokenKey = "p2g_token";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;
        try { token = await js.InvokeAsync<string?>("localStorage.getItem", TokenKey); }
        catch { /* prerender / SSR - JS not yet available */ }

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var principal = jwtService.ValidateToken(token);
        if (principal is null)
        {
            await ClearTokenAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(principal);
    }

    public async Task SetTokenAsync(string token)
    {
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task ClearTokenAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    // ✅ Add this method to retrieve the token
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            // JS not available (prerendering)
            return null;
        }
    }
}