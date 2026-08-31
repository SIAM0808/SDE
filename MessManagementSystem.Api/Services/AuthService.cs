using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs.Auth;
using MessManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MessManagementSystem.Api.Services;

public class AuthService
{
    private readonly MessDbContext _context;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;
    public AuthService(
        MessDbContext context,
        PasswordService passwordService,
        IConfiguration configuration
    )
    {
        _context = context;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    // Method to register a new member
    public async Task<Member> RegisterAsync(RegisterRequest request)
    {
        var existingMember = await _context.Members
            .FirstOrDefaultAsync(m => m.Email == request.Email);

        if (existingMember != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var passwordHash = _passwordService.HashPassword(request.Password);

        var member = new Member
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            PasswordHash = passwordHash,
            JoinDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Members.Add(member);

        await _context.SaveChangesAsync();

        return member;
    }


    // Method to authenticate a member and generate a JWT token
    public async Task<string> LoginAsync(LoginRequest request)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Email == request.Email);

        if (member == null)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        var passwordIsValid = _passwordService.VerifyPassword(
            request.Password,
            member.PasswordHash);

        if (!passwordIsValid)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        if (!member.IsActive)
        {
            throw new InvalidOperationException("This account is inactive.");
        }

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
        new Claim(ClaimTypes.Name, member.Name),
        new Claim(ClaimTypes.Email, member.Email)
    };

        var jwtKey = _configuration["Jwt:Key"];
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}