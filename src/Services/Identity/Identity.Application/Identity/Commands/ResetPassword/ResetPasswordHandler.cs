using Identity.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Identity.Application.Identity.Commands.ResetPassword
{
    public sealed class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand, Unit>
    {
        private readonly UserManager<User> _userManager;
        private readonly HttpClient _httpClient;

        public ResetPasswordHandler(UserManager<User> userManager, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _httpClient = httpClientFactory.CreateClient("NotificationAPI");
        }

        public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
                throw new DomainException("User not found");

            var newPassword = GenerateSecurePassword(12);
            // RESET PASSWORD
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!resetResult.Succeeded)
            {
                throw new DomainException($"Failed to change password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
            }
            // SEND EMAIL
            var emailRequest = new
            {
                to = user.Email,
                subject = "Password Reset Successful",
                body = GeneratePasswordResetEmail(user.FirstName.Concat(" " + user.LastName).ToString(), newPassword),
                isHtml = true
            };

            var response = await _httpClient.PostAsJsonAsync("/sendmail", emailRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new DomainException("Failed to send email");
            }
            return Unit.Value;
        }
        private static string GenerateSecurePassword(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
            var passwordChars = new char[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);
                for (int i = 0; i < length; i++)
                {
                    passwordChars[i] = validChars[byteBuffer[i] % validChars.Length];
                }
            }
            return new string(passwordChars);
        }
        public string GeneratePasswordResetEmail(string user, string newPassword)
        {
            string loginUrl = "https://localhost:7105";
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
              <p>Dear {user},</p>
              <p>We have received a request to reset your password. Your password has been successfully reset.</p>
              <p>Your new password is: <strong>{newPassword}</strong></p>
              <p>Please click the button below to log in to your account and update your password if desired.</p>
              <a href=""{loginUrl}"" class=""button"">Login to Your Account</a>
              <p>If you did not request this change, please contact our support team immediately.</p>
              <p>Best regards,</p>
              <p>{companyName}</p>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td class=""footer"">
              <p>{companyName} | {address}</p>
              <p>© 2025 {companyName}. All rights reserved.</p>
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
