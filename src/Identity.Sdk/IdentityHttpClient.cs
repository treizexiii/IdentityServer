using System.Net;
using System.Text;
using System.Text.Json;
using Identity.Wrappers.Dto;
using Identity.Wrappers.Messages;

namespace Identity.Sdk;

public class IdentityHttpClient(HttpClient client) : IIdentityClient
{
    public async Task<ApiResponse> LoginAsync(LoginDto loginDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Auth/login", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<JwtToken>>(responseContent);
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> RefreshTokenAsync()
    {
        var response = await client.PostAsync("api/Auth/refresh", null);
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse<JwtToken>>(responseContent);
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> LogoutAsync()
    {
        var response = await client.PostAsync("api/Auth/logout", null);
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse>(responseContent);
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> RegisterAsync(RegisterDto registerDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Auth/register", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse>(responseContent);
        if (data == null) throw new WebException("Invalid response");
        return data;
    }

    public async Task<ApiResponse> RegisterAppAsync(RegisterAppDto registerAppDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(registerAppDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("api/Admin/register-app", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<ApiResponse>(responseContent);
        if (data == null) throw new WebException("Invalid response");
        return data;
    }
}