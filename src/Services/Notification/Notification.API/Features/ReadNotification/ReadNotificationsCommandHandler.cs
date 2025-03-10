using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Features.ReadNotification
{
    public record ReadNotificationCommand(Guid NotificationId, Guid UserId) : ICommand<Unit>;
    public class ReadNotificationsCommandValidator : AbstractValidator<ReadNotificationCommand>
    {
        public ReadNotificationsCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.NotificationId).NotEmpty().WithMessage("NotificationId is required");
        }
    }
    public class ReadNotificationsCommandHandler : ICommandHandler<ReadNotificationCommand, Unit>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;
        public ReadNotificationsCommandHandler(NotificationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<Unit> Handle(ReadNotificationCommand command, CancellationToken cancellationToken)
        {
            var notification = await context.MessageNotifications.FirstOrDefaultAsync(mn => mn.Id == command.NotificationId);
            if (notification == null)
            {
                throw new NotFoundException("Notifications", command.NotificationId);
            }
            if (notification.Receiver != command.UserId)
            {
                throw new BadRequestException("Notification not belongs to user");
            }
            notification.IsRead = true;
            context.MessageNotifications.Update(notification);
            await context.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
