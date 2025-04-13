using BuildingBlocks.Messaging.Events;
using Google.Apis.Auth;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Application.Identity.Commands.SignupWithGoogle
{
    public sealed class SignupHandler : ICommandHandler<SignupCommand, SignupResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly GoogleSettings _googleSettings;
        private readonly EndpointSettings _emailSettings;
        private readonly ILogger<SignupHandler> _logger;
        private readonly IPublishEndpoint _publisher;

        public SignupHandler(IUserRepository userRepository, IOptions<GoogleSettings> googleSettings, IOptions<EndpointSettings> emailSettings, ILogger<SignupHandler> logger, IPublishEndpoint publisher)
        {
            _userRepository = userRepository;
            _googleSettings = googleSettings.Value;
            _logger = logger;
            _emailSettings = emailSettings.Value;
            _publisher = publisher;
        }

        public async Task<SignupResult> Handle(SignupCommand command, CancellationToken cancellationToken)
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

                var isExist = await _userRepository.GetUserByEmailAsync(payload.Email);
                if (isExist != null)
                {
                    //var newInfo = new UserLoginInfo("Google", payload.Subject, "Google");

                    //var changingLogins = await _userRepository.GetLoginsAsync(isExist);
                    //if (!changingLogins.Any(l => l.LoginProvider == "Google"))
                    //{
                    //    var addLoginResult = await _userRepository.AddLoginAsync(isExist, changingLogins);
                    //    if (!addLoginResult.Succeeded)
                    //    {
                    //        throw new Exception("Failed to link Google login.");
                    //    }
                    //}
                    //return new SignupResult(isExist.Id);
                    throw new DomainException("User already existing.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    Email = payload.Email,
                    UserName = payload.Email.Split('@')[0].Replace(".", "").Replace("-", "").Replace("_", "").ToLower(),
                    PhoneNumber = command.Phone,
                    BirthDate = command.BirthDate,
                    EmailConfirmed = true,
                    Gender = Enum.TryParse<Gender>(command.Gender, out var gender) ? gender : Gender.Other,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userRepository.CreateUserByGoogleAsync(user);

                if (!result.Succeeded)
                {
                    throw new DomainException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                var loginInfo = new UserLoginInfo("Google", payload.Subject, "Google");

                var existingLogins = await _userRepository.GetLoginsAsync(user);
                if (!existingLogins.Any(l => l.LoginProvider == "Google"))
                {
                    var addLoginResult = await _userRepository.AddLoginAsync(user, loginInfo);
                    if (!addLoginResult.Succeeded)
                    {
                        throw new Exception("Failed to link Google login.");
                    }
                }

                // Send gmail
                _publisher.Publish(new SendMailEvent(payload.Email, GenerateAnnouncement(), "Đăng ký thành công dịch vụ của SCRMS", true));

                _logger.LogInformation("User {Email} registered successfully via Google.", user.Email);
                return new SignupResult(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google signup: {Message}", ex.Message);
                throw new DomainException($"Error when create: {string.Join(", ", ex.Message)}");

            }
        }

        private static string GenerateAnnouncement()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Thông Báo</title>
    <style>
        
    </style>
</head>
<body>
    <div style=""  margin:0 auto; max-width: 600px;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            text-align: center;"">
        <h1 style=""
            color: #333;
            margin-bottom: 20px;"">Thông Báo</h1>
        <p>Kính gửi Quý khách hàng,</p>
        <p>Cảm ơn bạn đã tạo tài khoản bên chúng tôi.</p>
        <div style="" margin-top: 20px;
            font-size: 14px;
            color: #999;"">
            <p>Trân trọng,<br>Đội ngũ SCRMS</p>
        </div>
    </div>
</body>
</html>
";
        }
    }
}
