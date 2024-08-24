using API.Model;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwtController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUsersService _usersService;

        public JwtController(IConfiguration configuration, IUsersService usersService)
        {
            _configuration = configuration;
            _usersService = usersService;
        }

        [HttpPost("GenerateToken")]
        public IActionResult GenerateToken([FromBody] UserLoginDto user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Invalid user request");
            }

            var validUser = _usersService.GetUserInfo(user.Username, user.Password);
            if (validUser == null)
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(validUser);
            return Ok(token);
        }

        private string GenerateJwtToken(UserLoginDto user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("ValidateToken")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var jwtConfig = _configuration.GetSection("Jwt").Get<Jwt>();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key));
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = key
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Ok(new { IsValid = true, Claims = principal.Claims });
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { IsValid = false, Message = "Token has expired." });
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return Unauthorized(new { IsValid = false, Message = "Invalid token signature." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { IsValid = false, Message = ex.Message });
            }
        }
    }
}
