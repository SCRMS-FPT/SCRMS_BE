using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;
using Notification.API.Features.GetNotifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Notification.API.Features.ReadAllNotification
{
    public record ReadAllNotificationCommand(Guid UserId) : ICommand<Unit>;
    public class ReadAllNotificationCommandValidator : AbstractValidator<ReadAllNotificationCommand>
    {
        public ReadAllNotificationCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        }
    }
    public class ReadAllNotificationCommandHandler : ICommandHandler<ReadAllNotificationCommand, Unit>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;
        public ReadAllNotificationCommandHandler(NotificationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<Unit> Handle(ReadAllNotificationCommand command, CancellationToken cancellationToken)
        {
            var notifications = await context.MessageNotifications.Where(mn => mn.Receiver == command.UserId
                && !mn.IsRead).ToListAsync(cancellationToken);
            if (notifications != null)
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                    context.MessageNotifications.Update(notification);
                }
                await context.SaveChangesAsync();
            }
            return Unit.Value;
        }
    }
}
