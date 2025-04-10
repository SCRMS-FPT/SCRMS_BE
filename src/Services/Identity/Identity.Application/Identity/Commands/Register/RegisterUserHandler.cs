﻿using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Identity.Application.Identity.Commands.Register
{
    public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, RegisterUserResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly EndpointSettings _endpointSettings;
        private readonly IHttpClientFactory? _httpClientFactory;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IOptions<EndpointSettings> endpointSettings,
            IHttpClientFactory? httpClientFactory = null)
        {
            _userRepository = userRepository;
            _endpointSettings = endpointSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<RegisterUserResult> Handle(
            RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                UserName = command.Email,
                PhoneNumber = command.Phone,
                BirthDate = command.BirthDate,
                Gender = Enum.Parse<Gender>(command.Gender),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.CreateUserAsync(user, command.Password);

            if (!result.Succeeded)
            {
                throw new DomainException(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            // Skip email sending in test environments when httpClientFactory is null
            if (_httpClientFactory != null)
            {
                try
                {
                    // Send email 
                    HttpClient httpClient = _httpClientFactory.CreateClient();
                    var requestData = new
                    {
                        to = command.Email,
                        subject = "Thư xác minh tài khoản của SCRMS",
                        body = GenerateVerificationEmail(command.Email, _endpointSettings.Verification + GenerateToken(command.Email, _endpointSettings.VerificationKey)),
                        isHtml = true,
                    };
                    string jsonContent = JsonSerializer.Serialize(requestData);
                    StringContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    await httpClient.PostAsync(_endpointSettings.SendEmail, httpContent, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the registration
                    Console.WriteLine($"Failed to send verification email: {ex.Message}");
                }
            }

            return new RegisterUserResult(user.Id);
        }

        /// <summary>
        /// Generate the token to encrypt the data for sending
        /// </summary>
        /// <param name="email">This is the private data for backend verification</param>
        /// <param name="secretKey">This is the key to encrypt and decrypt</param>
        /// <returns>An token that can be decrypt using the sercet key, but safe with backend user</returns>
        public static string GenerateToken(string email, string secretKey)
        {
            var data = $"{email}|{DateTime.UtcNow.Ticks}";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var hashString = Convert.ToBase64String(hash);
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{data}|{hashString}"));
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
        <div style=""max-width: 600px; margin: 0 auto; background: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);"">
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