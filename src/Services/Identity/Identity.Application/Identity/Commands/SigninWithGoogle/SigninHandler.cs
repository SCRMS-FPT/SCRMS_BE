using Google.Apis.Auth;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Application.Identity.Commands.SigninWithGoogle
{
    public sealed class SigninHandler : ICommandHandler<SigninCommand, SigninResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly GoogleSettings _googleSettings;
        private readonly ILogger<SigninHandler> _logger;
        private readonly JwtSettings _jwtSettings;

        public SigninHandler(
            IUserRepository userRepository,
           IOptions<GoogleSettings> googleSettings,
            ILogger<SigninHandler> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _googleSettings = googleSettings.Value;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<SigninResult> Handle(SigninCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(command.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.Id }
                });

                if (payload == null)
                {
                    throw new UnauthorizedAccessException("Invalid Google token.");
                }

                var user = await _userRepository.GetUserByEmailAsync(payload.Email);

                if (user == null)
                {
                    throw new DomainException("Invalid credentials");
                }

                //if (!user.EmailConfirmed)
                //{
                //    throw new DomainException("This account doesn't connect to an gmail.");
                //}

                _logger.LogInformation("User {Email} logged in successfully via Google.", user.Email);

                var token = await GenerateJwtToken(user);
                var roles = await _userRepository.GetRolesAsync(user);

                var userDto = user.Adapt<UserDto>() with { Roles = roles.ToList() };
                return new SigninResult(
                    Token: token,
                    UserId: user.Id,
                    User: userDto
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google sign-in: {Message}", ex.Message);
                throw new DomainException("Google sign-in failed.");
            }
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
