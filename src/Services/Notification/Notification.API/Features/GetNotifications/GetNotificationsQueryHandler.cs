using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Features.GetNotifications
{
    public record GetNotificationsQuery(Guid UserId, int Page, int Limit, Boolean? IsRead, string? Type) : IQuery<List<NotificationResponse>>;
    public record NotificationResponse(Boolean IsRead, string Title, string Content, string Type, DateTime CreatedAt);
    public class GetNotifiesCommandValidator : AbstractValidator<GetNotificationsQuery>
    {
        public GetNotifiesCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.Page).NotEmpty().WithMessage("Page is required");
            RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be positive.");
            RuleFor(x => x.Limit).NotEmpty().WithMessage("Limit is required");
            RuleFor(x => x.Limit).GreaterThan(0).WithMessage("Limit must be positive.");
        }
    }
    public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, List<NotificationResponse>>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;

        public GetNotificationsQueryHandler(NotificationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<List<NotificationResponse>> Handle(GetNotificationsQuery command, CancellationToken cancellationToken)
        {
            var list = await context.MessageNotifications
                .Where(n => n.Receiver == command.UserId).ToListAsync(cancellationToken);

            if (command.IsRead != null)
            {
                list = list.Where(n => n.IsRead == command.IsRead).ToList();
            }

            if (command.Type != null)
            {
                list = list.Where(n => n.Type.Equals(command.Type)).ToList();
            }

            var result = list.Skip((command.Page - 1) * command.Limit)
             .Take(command.Limit)
             .Select(n => new NotificationResponse
             (
                 n.IsRead, n.Title, n.Content, n.Type, n.CreatedAt
             ))
             .ToList();

            return result;
        }
    }


}
