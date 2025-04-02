using Identity.Application.Data.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Identity.Domain.Exceptions;

namespace Identity.Application.Identity.Commands.RefreshToken
{
    public sealed class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenHandler(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<RefreshTokenResult> Handle(
            RefreshTokenCommand command,
            CancellationToken cancellationToken)
        {
            // Lưu ý: userId được lấy từ ClaimsPrincipal trong HTTP context
            // nên không cần truyền từ command

            // Không thể access HttpContext trực tiếp trong handler
            // UserId sẽ được truyền từ endpoint vào một parameter mới
            return command.UserId.HasValue
                ? await HandleWithUserId(command.UserId.Value)
                : throw new DomainException("User ID is required");
        }

        private async Task<RefreshTokenResult> HandleWithUserId(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new DomainException("User not found");

            var token = await GenerateJwtToken(user);
            var roles = await _userRepository.GetRolesAsync(user);

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
                roles.ToList()
            );

            return new RefreshTokenResult(
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