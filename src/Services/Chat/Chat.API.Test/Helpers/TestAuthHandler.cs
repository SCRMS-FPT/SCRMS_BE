using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Chat.API.Test
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public static ClaimsPrincipal? FakeUser { get; set; }

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (FakeUser == null)
                return Task.FromResult(AuthenticateResult.Fail("No FakeUser set"));

            var ticket = new AuthenticationTicket(FakeUser, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}