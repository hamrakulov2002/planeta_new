using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Auth;
using Planeta.Application.Interfaces;
using System.Security.Claims;

using MyRegisterRequest = Planeta.Application.DTOs.Auth.RegisterRequest;
using MyLoginRequest = Planeta.Application.DTOs.Auth.LoginRequest;

namespace Planeta_New.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authservice)
    {
        _authService = authservice;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] MyRegisterRequest request)
    {
        try
        {
            var responce = await _authService.RegisterAsync(request);
            return Ok(responce);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] MyLoginRequest request)
    {
        try
        {
            var responce = await _authService.LoginAsync(request);
            return Ok(responce);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin([FromQuery] string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl })
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
public async Task<IActionResult> GoogleCallback([FromQuery] string returnUrl = "/")
{
    var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (!result.Succeeded || result.Principal == null)
        return BadRequest(new { message = "Google authentication failed" });

    var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
    var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(email))
        return BadRequest(new { message = "Email claim not found from Google" });

    try
    {
        // Передаем данные в AuthService и получаем готовый JWT
        var authResponse = await _authService.GoogleLoginAsync(email, name ?? string.Empty, googleId ?? string.Empty);
        return Ok(authResponse);
    }
    catch (Exception ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
}