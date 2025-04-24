using BuildingBlocks.Messaging.Events;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Identity.Commands.Register
{
    public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly EndpointSettings _endpointSettings;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterUserHandler(IUserRepository userRepository,
            IOptions<EndpointSettings> endpointSettings,
            IPublishEndpoint publishEndpoint)
        {
            _userRepository = userRepository;
            _endpointSettings = endpointSettings.Value;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            // Validate
            var user = await _userRepository.GetUserByEmailAsync(command.Email);
            if (user != null)
            {
                throw new DomainException("Email already taken");
            }
            // Send email 
            await _publishEndpoint.Publish(
                new SendMailEvent(
                    command.Email,
                    GenerateVerificationEmail(command.FirstName + " " + command.LastName,
                    _endpointSettings.Verification + GenerateTokenWithHashedPassword(command, _endpointSettings.VerificationKey)),
                    "Thư xác minh tài khoản của SCRMS",
                    true));

            return Unit.Value;
        }

        public static string GenerateTokenWithHashedPassword(RegisterUserCommand command, string secretKey)
        {
            var registrationData = $"{command.FirstName}|{command.LastName}|{command.Email}|{command.Phone}|{command.BirthDate:yyyyMMdd}|{command.Gender}|{command.Password}";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registrationData));
                var hashString = Convert.ToBase64String(hash);
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{registrationData}|{hashString}"));

                return token;
            }
        }

        private static string GenerateVerificationEmail(string username, string link)
        {
            return $@"
    <!DOCTYPE html>
    <html lang=""vi"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                margin: 0;
                padding: 20px;
            }}
            .container {{
            }}
            .header {{
            }}
            .header h1 {{
                color: #333;
            }}
            .content {{
            }}
            .button {{
            }}
            .footer {{
            }}
        </style>
    </head>
    <body>
        <div style=""max-width: 600px; margin: 0 auto; background: #f2f2f2; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);"">
            <div style=""text-align: center; margin-bottom: 20px;"">
                <h1>Xác Minh Tài Khoản Sports Court Management and Reservation System</h1>
            </div>
            <div style=""font-size: 16px; line-height: 1.5; color: #555;"">
                <p>Kính gửi <strong>{username}</strong>,</p>
                <p>Cảm ơn bạn đã đăng ký tại SCMRS! Chúng tôi rất vui mừng khi có bạn là một phần của cộng đồng chúng tôi.</p>
                <p>Để hoàn tất quá trình đăng ký, vui lòng xác minh địa chỉ email của bạn bằng cách nhấp vào liên kết dưới đây:</p>
                <a href=""{link}"" style=""display: block; background-color: #007BFF; color: #ffffff; padding: 10px; text-decoration: none; border-radius: 5px; margin-top: 20px; text-align: center;"">
                    <p>Xác Minh Email</p>
                </a>
                <p>Nếu bạn không tạo tài khoản với chúng tôi, vui lòng bỏ qua email này.</p>
                <p>Cảm ơn bạn đã tham gia SCMRS! Nếu bạn có bất kỳ câu hỏi nào hoặc cần hỗ trợ, hãy liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
            </div>
            <div style=""margin-top: 20px; font-size: 14px; text-align: center; color: #999;"">
                <p>Trân trọng,<br>Đội ngũ SCMRS</p>
            </div>
        </div>
    </body>
    </html>";
        }

    }
}