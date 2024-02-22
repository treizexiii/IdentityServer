namespace Identity.Wrappers.Dto;

public class JwtToken
{
    public string TokenType { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}