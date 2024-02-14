using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Core.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Core.Tools;

public class JwtTokenBuilder
{
    private readonly string _audience;
    private readonly string _issuer;
    private readonly byte[] _secretKey;

    private string? _id;
    private string? _username;
    private string? _email;
    private string? _role;
    private string? _provider;

    private readonly Dictionary<string, string> _customClaims = new();

    public JwtTokenBuilder(string issuer, string audience, string secretKey)
    {
        _issuer = issuer;
        _audience = audience;
        _secretKey = Encoding.UTF8.GetBytes(secretKey);
    }

    public JwtTokenBuilder AddUser(User user)
    {
        _id = user.Id.ToString();
        _username = user.UserName;
        _email = user.Email;

        return this;
    }

    public JwtTokenBuilder AddRole(Role role)
    {
        _role = role.Name;

        return this;
    }

    public void AddProvider(string provider)
    {
        _provider = provider;
    }

    public void AddCustomClaim(string type, string value)
    {
        _customClaims.Add(type, value);
    }

    public string BuildJwtToken(int expireMinutes = 30)
    {
        var key = new SymmetricSecurityKey(_secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor();
        var claims = new List<Claim>();
        var expire = DateTimeOffset.UtcNow.AddMinutes(expireMinutes);

        if (!string.IsNullOrEmpty(_id))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, _id));
        if (!string.IsNullOrEmpty(_username))
            claims.Add(new Claim(ClaimTypes.Name, _username));
        if (!string.IsNullOrEmpty(_email))
            claims.Add(new Claim(ClaimTypes.Email, _email));
        if (!string.IsNullOrEmpty(_role))
            claims.Add(new Claim(ClaimTypes.Role, _role));
        if (!string.IsNullOrEmpty(_provider))
            claims.Add(new Claim(ClaimTypes.Spn, _provider));

        claims.Add(new Claim(ClaimTypes.Expiration, expire.ToUnixTimeSeconds().ToString()));
        claims.AddRange(_customClaims.Select(claim => new Claim(claim.Key, claim.Value)));

        tokenDescriptor.Subject = new ClaimsIdentity(claims);
        tokenDescriptor.Issuer = _issuer;
        tokenDescriptor.Audience = _audience;
        tokenDescriptor.SigningCredentials = credentials;
        tokenDescriptor.Expires = expire.DateTime;

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string BuildRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;
        var claims = decodedToken?.Claims.ToArray();
        if (claims is null) throw new UnauthorizedAccessException("Invalid token");

        var nameId = claims.FirstOrDefault(x => x.Type == RawClaimsType.NameIdentifier)?.Value;
        var username = claims.FirstOrDefault(x => x.Type == RawClaimsType.Name)?.Value;
        var role = claims.FirstOrDefault(x => x.Type == RawClaimsType.Role)?.Value;

        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Invalid token");
        if (string.IsNullOrEmpty(nameId))
            throw new UnauthorizedAccessException("Invalid token");
        if (string.IsNullOrEmpty(role))
            throw new UnauthorizedAccessException("Invalid token");

        var tic = long.Parse(claims.FirstOrDefault(x => x.Type == RawClaimsType.Expiration).Value);
        var expire = DateTimeOffset.FromUnixTimeSeconds(tic).UtcDateTime;

        if (expire.AddDays(1) < DateTime.UtcNow) throw new UnauthorizedAccessException("Token expired");

        return BuildJwtToken(43200);
    }

    public Claim[] ControlToken(string token)
    {
        var claims = GetClaims(token).ToArray();
        if (claims is null) throw new UnauthorizedAccessException("Invalid token");

        var username = claims.FirstOrDefault(x => x.Type == "name")?.Value;
        if (string.IsNullOrEmpty(username)) throw new UnauthorizedAccessException("Invalid token");

        // control token signature
        var key = new SymmetricSecurityKey(_secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = credentials.Key,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);
        if (principal is null) throw new UnauthorizedAccessException("Invalid token");

        return claims;
    }

    private IEnumerable<Claim> GetClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;
        var claims = decodedToken?.Claims;
        if (claims is null) throw new UnauthorizedAccessException("Invalid token");

        return claims;
    }
}

public static class RawClaimsType
{
    public const string NameIdentifier = "nameid";
    public const string Name = "unique_name";
    public const string Email = "email";
    public const string Role = "role";
    public const string Expiration = "exp";
}