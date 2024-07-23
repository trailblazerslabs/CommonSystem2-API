using CommonSystem2_API.Middleware;
using CommonSystem2_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CommonSystem2_API.Controllers;
public class AuthTestController : BaseController
{
    private readonly ILogger<AuthTestController> _logger;
    private readonly IUserService _authService;
    private readonly IConfiguration _configuration;

    public AuthTestController(IUserService authService,
        ILogger<AuthTestController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _authService = authService;
    }

    [HttpGet("Test")]
    [AuthorizeRoles("Admin")]
    public IActionResult Test()
    {
        if (Request.Headers.TryGetValue("Authorization", out var tokenValues))
        {
            string accessToken = tokenValues.ToString().Split(' ')[1];
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(accessToken) as JwtSecurityToken;
            return Ok(new
            {
                message = "The endpoint is authenticated",
                token = token
            });
        }
        return Ok(new { message = "This endpoint should require authentication and a specific permission, and should return a string which lists the user's organisation followed by their permissions." });
    }

    [HttpGet("isValidToken")]
    public IActionResult IsValidToken()
    {
        var userRoles = HttpContext.User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            message = "The endpoint is authenticated",
            roles = userRoles
        });
    }
}