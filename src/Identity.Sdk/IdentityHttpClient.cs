using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Identity.Wrappers.Dto;
using Identity.Wrappers.Messages;

namespace Identity.Sdk;

public class IdentityHttpClient(HttpClient client) : IIdentityClient
{
    public async Task<ApiResponse<JwtToken>> LoginAsync(LoginDto loginDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Auth/login", content);
        // response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<ApiResponse<JwtToken>>();
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse<JwtToken>> RefreshTokenAsync()
    {
        var response = await client.PostAsync("api/Auth/refresh", null);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<ApiResponse<JwtToken>>();
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        var response = await client.PostAsync("api/Auth/logout", null);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<ApiResponse>();
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> RegisterAsync(RegisterDto registerDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Auth/register", content);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<ApiResponse>();
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> RegisterAppAsync(RegisterAppDto registerAppDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(registerAppDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Admin/register-app", content);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<ApiResponse>();
        if (data == null) throw new WebException("Invalid response");
        return data;
    }
}