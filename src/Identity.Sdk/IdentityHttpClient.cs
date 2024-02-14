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
        var response = await client.PostAsync("login", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<JwtToken>>(responseContent);
    }

    public async Task<ApiResponse> RegisterAsync(RegisterDto registerDto)
    {
        var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("register", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(responseContent);
    }

    public Task<ApiResponse> RegisterAppAsync(RegisterAppDto registerAppDto)
    {
        throw new NotImplementedException();
    }
}

