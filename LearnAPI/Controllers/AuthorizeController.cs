using LearnAPI.Modal;
using LearnAPI.Repos;
using LearnAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LearnAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly LearndataContext context;
        private readonly IRefreshHandler refresh;
        private readonly JwtSettings jwtSettings;

        public AuthorizeController(LearndataContext context, IOptions<JwtSettings> options, IRefreshHandler refresh)
        {
            this.context = context;
            this.refresh = refresh;
            this.jwtSettings = options.Value;
        }

        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCred userCred)
        {
            var user = await this.context.TblUsers.FirstOrDefaultAsync(item =>
                item.Username == userCred.username && item.Password == userCred.password);
            if (user != null)
            {
                // generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.securitykey);
                var tokendesc = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokendesc);
                var finaltoken = tokenHandler.WriteToken(token);
                return Ok(new TokenResponse() { Token = finaltoken, RefreshToken = await this.refresh.GenerateToken(userCred.username), userRole = user.Role});
            }
            else
            {
                return Unauthorized();
            }
        }

        // ... other code ...

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateToken([FromBody] TokenResponse token)
        {
            var _refreshToken = await this.context.TblRefreshtokens.FirstOrDefaultAsync(item =>
                item.Refreshtoken == token.RefreshToken);
            if (_refreshToken != null)
            {
                // generate token
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenkey = Encoding.UTF8.GetBytes(this.jwtSettings.securitykey);
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenkey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidateLifetime = false
                }, out securityToken);

                var _token = securityToken as JwtSecurityToken;
                if (_token != null && _token.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                {
                    string username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    var _existData = await this.context.TblRefreshtokens.FirstOrDefaultAsync(item =>
                        item.UserId == username && item.Refreshtoken == token.RefreshToken);
                    if (_existData != null)
                    {
                        var _newtoken = new JwtSecurityToken(
                            claims: principal.Claims.ToArray(),
                            expires: DateTime.UtcNow.AddHours(1),
                            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.securitykey)),
                                SecurityAlgorithms.HmacSha256)
                            );
                        var _finaltoken = tokenHandler.WriteToken(_newtoken);
                        return Ok(new TokenResponse() { Token = _finaltoken, RefreshToken = await this.refresh.GenerateToken(username) });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Unauthorized();
            }
        }
    }
}
