using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Features.DeleteNotification
{
    public record DeleteNotificationCommand(Guid NotificationId, Guid UserId) : ICommand<DeleteNotificationResponse>;
    public record DeleteNotificationResponse(Boolean IsDeleted);
    public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
    {
        public DeleteNotificationCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.NotificationId).NotEmpty().WithMessage("NotificationId is required");
        }
    }
    internal class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand, DeleteNotificationResponse>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;

        public DeleteNotificationCommandHandler(NotificationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<DeleteNotificationResponse> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken)
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
            context.MessageNotifications.Remove(notification);
            await context.SaveChangesAsync();
            return new DeleteNotificationResponse(true);

        }
    }
}
