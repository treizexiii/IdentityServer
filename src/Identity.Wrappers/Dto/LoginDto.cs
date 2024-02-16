
namespace Identity.Wrappers.Dto;

public record LoginDto(string Username, string Password, string? AppKey);

public record RegisterDto(string Email, string Password, string AppKey);

public record RegisterAppDto(string Email, string Password, string AppName)
{
    public RegisterDto ToRegisterRequest(string appKey)
    {
        return new RegisterDto(Email, Password, appKey);
    }
}

