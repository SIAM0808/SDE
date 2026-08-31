using MessManagementSystem.Api.DTOs.Auth;
using MessManagementSystem.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var member = await _authService.RegisterAsync(request);

            return Ok(new
            {
                message = "Registration successful.",
                memberId = member.Id,
                name = member.Name,
                email = member.Email
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request);

            return Ok(new
            {
                message = "Login successful.",
                token = token
            });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }
}