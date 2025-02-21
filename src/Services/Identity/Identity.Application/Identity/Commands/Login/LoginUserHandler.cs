using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Application.Identity.Commands.Login
{
    public sealed class LoginUserHandler : ICommandHandler<LoginUserCommand, LoginUserResult>
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtSettings _jwtSettings;

        public LoginUserHandler(
            UserManager<User> userManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginUserResult> Handle(
            LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, command.Password))
            {
                throw new DomainException("Invalid credentials");
            }

            var token = await GenerateJwtToken(user);

            return new LoginUserResult(
                Token: token,
                UserId: user.Id,
                Email: user.Email
            );
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}