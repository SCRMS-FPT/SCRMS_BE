using Identity.Application.Data.Repositories;
using Identity.Domain.Events;
using Identity.Domain.Exceptions;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Identity.Queries.Verification
{
    public class VerificationHandler : IQueryHandler<VerificationQuery, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly EndpointSettings _endpointSettings;
        private readonly IPublishEndpoint _publishEndpoint;
        public VerificationHandler(
            IUserRepository userRepository,
            IOptions<EndpointSettings> endpointSettings,
            IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _endpointSettings = endpointSettings.Value;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(
           VerificationQuery request,
           CancellationToken cancellationToken)
        {
            var userData = ValidateTokenAndExtractUser(request.Token, _endpointSettings.VerificationKey);
            if (userData == null)
                throw new DomainException("Token is not valid or has been tampered with.");

            // Check if user already exists
            var existingUser = await _userRepository.GetUserByEmailAsync(userData.Email);
            if (existingUser != null)
                throw new DomainException("User already exists or has already been verified.");

            // Create and save the user
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                Email = userData.Email,
                UserName = userData.Email,
                PhoneNumber = userData.Phone,
                BirthDate = userData.BirthDate,
                Gender = Enum.Parse<Gender>(userData.Gender),
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.CreateUserAsync(newUser, userData.Password);
            if (!result.Succeeded)
                throw new DomainException("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            await _publishEndpoint.Publish(new UserCreatedEvent(newUser.Id));
            return Unit.Value;
        }

        private static UserTokenModel? ValidateTokenAndExtractUser(string token, string secretKey)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = decoded.Split('|');

                if (parts.Length != 8)
                    return null;

                var (firstName, lastName, email, phone, birthDateStr, gender, password, providedHash) =
                        (parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7]);

                var payload = string.Join('|', parts.Take(7));

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

                if (!computedHash.Equals(providedHash))
                {
                    return null;
                }

                return new UserTokenModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    BirthDate = DateTime.SpecifyKind(
                        DateTime.ParseExact(birthDateStr, "yyyyMMdd", CultureInfo.InvariantCulture),
                        DateTimeKind.Utc
                    ),
                    Gender = gender,
                    Password = password
                };
            }
            catch
            {
                return null;
            }
        }

        private class UserTokenModel
        {
            public string Email { get; set; } = default!;
            public string FirstName { get; set; } = default!;
            public string LastName { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public DateTime BirthDate { get; set; }
            public string Gender { get; set; } = default!;
            public string Password { get; set; } = default!;
        }
    }
}
