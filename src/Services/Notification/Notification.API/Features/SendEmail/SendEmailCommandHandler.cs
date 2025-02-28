using BuildingBlocks.CQRS;
using FluentValidation;
using Notification.API.Services;

namespace Notification.Api.Features.SendEmail
{
    public record SendEmailCommand(string To, string Subject, string Body) : ICommand<SendEmailResult>;
    public record SendEmailResult(Boolean Sent);
    public class SendEmailCommandValidator : AbstractValidator<SendEmailCommand>
    {
        public SendEmailCommandValidator()
        {
            RuleFor(x => x.To)
                .NotEmpty().WithMessage("Recipient email is required.")
                .Matches(@"^[a-zA-Z0-9._%+-]+@gmail\.com$").WithMessage("Recipient email must be a valid Gmail address.");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Email subject is required.");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Email body is required.");
        }
    }
    public class SendEmailCommandHandler : ICommandHandler<SendEmailCommand, SendEmailResult>
    {
        private readonly IEmailService _emailService;

        public SendEmailCommandHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<SendEmailResult> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            var result = new SendEmailResult(await _emailService.SendEmailAsync(request.To, request.Subject, request.Body));
            return result;
        }
    }
}
