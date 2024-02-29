namespace Identity.Wrappers.Dto;

public record LoginDto(string Username, string Password);

public record RegisterDto(string Email, string Password, string AppKey);

public record RegisterAppDto(string Email, string Password, string AppName, string? Description)
{
    public RegisterDto ToRegisterRequest(string appKey)
    {
        return new RegisterDto(Email, Password,  appKey);
    }
}

public record AppDto(Guid Id, string AppName, string ApiKey, string Secret);

