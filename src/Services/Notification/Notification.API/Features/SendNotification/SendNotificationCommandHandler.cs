using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Notification.Api.Features.SendEmail;
using Notification.API.Data;
using Notification.API.Data.Model;
using Notification.API.Hubs;

namespace Notification.API.Features.SendNotification
{
    public record SendNotificationCommand(
        Guid SendTo,
        string Title,
        string Content,
        string Type,
        Boolean SendMail,
        string UserEmail
        ) : ICommand<Unit>;

    public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
    {
        public SendNotificationCommandValidator()
        {
            RuleFor(x => x.SendTo)
                .NotEmpty().WithMessage("Notifications's recipient is required.");
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Notifications's title is required.");
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Notifications's content is required.");
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Notifications's type is required.");
        }
    }
    public class SendNotificationCommandHandler : ICommandHandler<SendNotificationCommand, Unit>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;
        private readonly ISender sender;
        private readonly IHubContext<NotifyHub> hubContext;
        public SendNotificationCommandHandler(NotificationDbContext context, IMediator mediator, ISender sender, IHubContext<NotifyHub> hubContext)
        {
            this.context = context;
            this.mediator = mediator;
            this.sender = sender;
            this.hubContext = hubContext;
        }

        public async Task<Unit> Handle(SendNotificationCommand command, CancellationToken cancellationToken)
        {
            // Save to db
            MessageNotification notification = new MessageNotification()
            {
                Title = command.Title,
                Content = command.Content,
                Type = command.Type,
                Receiver = command.SendTo
            };
            context.MessageNotifications.Add(notification);
            await context.SaveChangesAsync();
            // Realtime using signalR
            await hubContext.Clients.User(command.SendTo.ToString()).SendAsync("ReceiveNotification", notification);
            // Send email
            if (command.SendMail)
            {
                var emailCommand = new SendEmailCommand(
                    command.UserEmail,
                    $"{command.Title}",
                command.Content,
                false
           );

                await sender.Send(emailCommand, cancellationToken);

            }

            return Unit.Value;

        }
    }
}
