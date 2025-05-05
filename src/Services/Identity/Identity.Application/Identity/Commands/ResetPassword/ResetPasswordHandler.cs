using BuildingBlocks.Messaging.Events;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public sealed class ResetPasswordHandler :
        ICommandHandler<RequestPasswordResetCommand, Unit>,
        ICommandHandler<ConfirmPasswordResetCommand, Unit>
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpClient _httpClient;
        private readonly IUserRepository _userRepository;
        private readonly EndpointSettings _endpointSettings;
        private readonly IPublishEndpoint _publishEndpoint;

        public ResetPasswordHandler(UserManager<User> userManager, IHttpClientFactory httpClientFactory, IUserRepository userRepository, IOptions<EndpointSettings> endpointSettings,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _httpClient = httpClientFactory.CreateClient("NotificationAPI");
            _userRepository = userRepository;
            _endpointSettings = endpointSettings.Value;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmailAsync(command.Email);
            if (user == null)
                throw new DomainException("Người dùng không tồn tại");
            if (user.IsDeleted)
                throw new DomainException("Người dùng không tồn tại");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{_endpointSettings.ResetPassword}/{Uri.EscapeDataString(GenerateAnCodeForResetPassword(command.Email, token, _endpointSettings.VerificationKey))}";

            await _publishEndpoint.Publish(
               new SendMailEvent(
                   user.Email,
                   GeneratePasswordResetEmail(user.FirstName + " " + user.LastName, resetLink),
                   "Yêu cầu đặt lại mật khẩu",
                   true));

            return Unit.Value;
        }

        public async Task<Unit> Handle(ConfirmPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var response = DecryptModel(command.Token, _endpointSettings.VerificationKey);
            if (response == null)
            {
                throw new DomainException("Token không hợp lệ hoặc đã hết hạn");
            }
            var user = await _userRepository.GetUserByEmailAsync(response.Email);
            if (user == null)
                throw new DomainException("Token không hợp lệ hoặc đã hết hạn");

            var result = await _userManager.ResetPasswordAsync(user, response.Token, command.NewPassword);
            if (!result.Succeeded)
                throw new DomainException("Không thể thay đổi mật khẩu: " +
                                          string.Join(", ", result.Errors.Select(e => e.Description)));

            return Unit.Value;
        }

        public static string GenerateAnCodeForResetPassword(string email, string token, string secretKey)
        {
            var resetData = $"{email}|{token}";

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(resetData));
                var hashString = Convert.ToBase64String(hash);
                var code = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resetData}|{hashString}"));

                return code;
            }
        }

        private static ResetModel? DecryptModel(string code, string secretKey)
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(code));
                var parts = decoded.Split('|');

                if (parts.Length != 3)
                    return null;

                var (email, token, providedHash) =
                        (parts[0], parts[1], parts[2]);

                var payload = string.Join('|', parts.Take(2));

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

                if (!computedHash.Equals(providedHash))
                {
                    return null;
                }

                return new ResetModel
                {
                    Email = email,
                    Token = token,
                };
            }
            catch
            {
                return null;
            }
        }

        private class ResetModel
        {
            public string Email { get; set; }
            public string Token { get; set; }
        }

        public string GeneratePasswordResetEmail(string user, string newLink)
        {
            string loginUrl = _endpointSettings.Login;
            string address = "Thach That, Hoa Lac, Ha Noi";
            string companyName = "Sports Court Management and Reservation System";

            string htmlTemplate = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Password Reset Notification</title>
  <style>
    /* Base styles */
    body {{
      margin: 0;
      padding: 0;
      background-color: #f2f2f2;
      font-family: Arial, sans-serif;
    }}
    table {{
      border-spacing: 0;
    }}
    td {{
      padding: 0;
    }}
    /* Wrapper table */
    .wrapper {{
      width: 100%;
      table-layout: fixed;
      background-color: #f2f2f2;
      padding: 50px 0;
    }}
    /* Main container */
    .main {{
      background-color: #ffffff;
      margin: 0 auto;
      width: 100%;
      max-width: 600px;
      border-radius: 5px;
      overflow: hidden;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }}
    /* Header section */
    .header {{
      background-color: #4a90e2;
      color: #ffffff;
      padding: 20px;
      text-align: center;
    }}
    /* Content section */
    .content {{
      padding: 30px 20px;
      color: #333333;
      font-size: 16px;
      line-height: 1.5;
    }}
    /* Button */
    .button {{
      display: block;
      width: 200px;
      margin: 20px auto;
      text-align: center;
      background-color: #4a90e2;
      color: #ffffff;
      text-decoration: none;
      padding: 15px;
      border-radius: 5px;
      font-weight: bold;
    }}
    /* Footer section */
    .footer {{
      padding: 20px;
      text-align: center;
      font-size: 12px;
      color: #777777;
    }}
  </style>
</head>
<body>
  <table class=""wrapper"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
    <tr>
      <td align=""center"">
        <table class=""main"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
          <!-- Header -->
          <tr>
            <td class=""header"">
              <h1>{companyName}</h1>
            </td>
          </tr>
          <!-- Content -->
         <tr>
          <td class=""content"">
            <p>Chào {user},</p>
            <p>Chúng tôi đã nhận được yêu cầu thay đổi mật khẩu từ bạn.</p>
            <p>Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu mới cho tài khoản của bạn.</p>
            <a href=""{newLink}"" class=""button"">Thay đổi mật khẩu</a>
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
            <p>Trân trọng,</p>
            <p>{companyName}</p>
          </td>
        </tr>
        <!-- Footer -->
        <tr>
          <td class=""footer"">
            <p>{companyName} | {address}</p>
            <p>© 2025 {companyName}. Bảo lưu mọi quyền.</p>
          </td>
        </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            return htmlTemplate;
        }
    }
}