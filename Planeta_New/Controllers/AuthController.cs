using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Planeta.Application.DTOs.Auth;
using Planeta.Application.Interfaces;

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
            return Unauthorized(new {message = ex.Message});
        }

    }

}
