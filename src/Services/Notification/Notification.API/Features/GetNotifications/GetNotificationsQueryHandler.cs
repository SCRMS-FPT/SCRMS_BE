using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Features.GetNotifications
{
    public record GetNotificationsQuery(Guid UserId, int Page, int Limit, Boolean? IsRead, string? Type) : IQuery<PaginatedResult<NotificationResponse>>;
    public record NotificationResponse(Guid Id, Boolean IsRead, string Title, string Content, string Type, DateTime CreatedAt);
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
    public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, PaginatedResult<NotificationResponse>>
    {
        private readonly NotificationDbContext context;
        private readonly IMediator mediator;

        public GetNotificationsQueryHandler(NotificationDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<PaginatedResult<NotificationResponse>> Handle(GetNotificationsQuery command, CancellationToken cancellationToken)
        {
            var list = await context.MessageNotifications
                .Where(n => n.Receiver == command.UserId).OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

            if (command.IsRead != null)
            {
                list = list.Where(n => n.IsRead == command.IsRead).ToList();
            }

            if (command.Type != null)
            {
                list = list.Where(n => n.Type.Equals(command.Type)).ToList();
            }

            var count = list.Count();

            var result = list.Skip((command.Page - 1) * command.Limit)
             .Take(command.Limit)
             .Select(n => new NotificationResponse
             (
               n.Id, n.IsRead, n.Title, n.Content, n.Type, n.CreatedAt
             ))
             .ToList();

            return new PaginatedResult<NotificationResponse>(command.Page, command.Limit, count, result);
        }
    }


}
