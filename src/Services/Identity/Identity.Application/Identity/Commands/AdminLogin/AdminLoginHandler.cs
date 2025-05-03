using Identity.Application.Data.Repositories;
using Identity.Application.Identity.Commands.Login;
using Identity.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Application.Identity.Commands.AdminLogin
{
    public sealed class AdminLoginHandler : ICommandHandler<AdminLoginCommand, AdminLoginResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AdminLoginHandler> _logger;

        public AdminLoginHandler(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AdminLoginHandler> logger)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<AdminLoginResult> Handle(
           AdminLoginCommand command,
           CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmailAsync(command.Email);

            if (user == null || !await _userRepository.CheckPasswordAsync(user, command.Password))
            {
                throw new DomainException("Invalid credentials");
            }
            if (user.IsDeleted)
            {
                throw new DomainException("Invalid credentials");
            }

            var roles = await _userRepository.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
            {
                throw new DomainException("Invalid credentials");
            }
            var token = await GenerateJwtToken(user);
            var userDto = new UserDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.BirthDate,
                user.Gender.ToString(),
                user.SelfIntroduction,
                user.CreatedAt,
                roles.ToList(),
                user.GetImageUrlsList()
            );
            return new AdminLoginResult(
                Token: token,
                UserId: user.Id,
                User: userDto
            );
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var roles = await _userRepository.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
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
