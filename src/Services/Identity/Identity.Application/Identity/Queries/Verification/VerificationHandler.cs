using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Identity.Queries.Verification
{
    public class VerificationHandler : IQueryHandler<VerificationQuery, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly EndpointSettings _endpointSettings;

        public VerificationHandler(IUserRepository userRepository, IOptions<EndpointSettings> endpointSettings)
        {
            _userRepository = userRepository;
            _endpointSettings = endpointSettings.Value;

        }

        public async Task<Unit> Handle(
           VerificationQuery request,
           CancellationToken cancellationToken)
        {
            var email = ValidateToken(request.Token, _endpointSettings.VerificationKey);
            if (email == null)
            {
                throw new DomainException("Token is not valid.");
            }
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new DomainException("User is not existed.");
            }
            var result = _userRepository.VerifyEmailAsync(user);
            if (result.Result.Errors.Any())
            {
                throw new DomainException("User's gmail is already verify.");
            }

            return Unit.Value;
        }

        private static string ValidateToken(string token, string secretKey)
        {
            try
            {
                var decodedBytes = Convert.FromBase64String(token);
                var decodedString = Encoding.UTF8.GetString(decodedBytes);
                var parts = decodedString.Split('|');

                if (parts.Length != 3) return null; // Token sai định dạng

                var email = parts[0];
                var timestamp = parts[1];
                var receivedHash = parts[2];

                var data = $"{email}|{timestamp}";
                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data)));

                    if (computedHash == receivedHash)
                    {
                        return email;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

    }
}
