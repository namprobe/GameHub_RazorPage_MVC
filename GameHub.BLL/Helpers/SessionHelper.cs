using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace GameHub.BLL.Helpers;

public class SessionHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private const string JWT_TOKEN_KEY = "jwt_token";
    private const string JWT_EXPIRES_KEY = "jwt_expires";

    public SessionHelper(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void SetJwtToken(string token, DateTime expiresAt)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.SetString(JWT_TOKEN_KEY, token);
            session.SetString(JWT_EXPIRES_KEY, expiresAt.ToString("O"));
        }
    }

    public string? GetJwtToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(JWT_TOKEN_KEY);
    }

    public DateTime? GetJwtExpires()
    {
        var expiresString = _httpContextAccessor.HttpContext?.Session.GetString(JWT_EXPIRES_KEY);
        if (DateTime.TryParse(expiresString, out var expires))
        {
            return expires;
        }
        return null;
    }

    public int? GetCurrentUserId()
    {
        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
            return null;

        var principal = JwtHelper.ValidateToken(token, _configuration);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
            
        return null;
    }

    public string? GetCurrentRole()
    {
        var token = GetJwtToken();
        if (string.IsNullOrEmpty(token))
            return null;
            
        var principal = JwtHelper.ValidateToken(token, _configuration);
        var roleClaim = principal?.FindFirst(ClaimTypes.Role)?.Value;
        return roleClaim;
    }

    public bool IsJwtValid()
    {
        var token = GetJwtToken();
        var expires = GetJwtExpires();
        return !string.IsNullOrEmpty(token) && expires.HasValue && expires.Value > DateTime.Now;
    }

    public void ClearSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session != null)
        {
            session.Remove(JWT_TOKEN_KEY);
            session.Remove(JWT_EXPIRES_KEY);
        }
    }
}
