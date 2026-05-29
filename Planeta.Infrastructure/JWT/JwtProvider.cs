using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Planeta.Domain.Auth;
using Planeta.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Planeta.Infrastructure.JWT;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        if (user.Role.Permissions != null)
        {
            foreach (var permission in user.Role.Permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }
        }

        
        var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret Key не найден в конфигурации.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

       
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(25),
            signingCredentials: creds
        );

        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
