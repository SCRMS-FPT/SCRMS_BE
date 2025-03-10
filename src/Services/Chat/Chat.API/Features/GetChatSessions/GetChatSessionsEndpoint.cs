using System.Security.Claims;

namespace Chat.API.Features.GetChatSessions
{
    public class GetChatSessionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/chats", async (
                ISender sender,
                HttpContext httpContext,
                int page,
                int limit) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.BadRequest("Invalid user ID in token");

                var query = new GetChatSessionsQuery(page, limit, userId);
                var validator = new GetChatSessionsQueryValidator();
                var validationResult = await validator.ValidateAsync(query);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetChatSessions");
        }

        public class GetChatSessionsQueryValidator : AbstractValidator<GetChatSessionsQuery>
        {
            public GetChatSessionsQueryValidator()
            {
                RuleFor(x => x.UserId)
                    .NotEmpty().WithMessage("UserId cannot be empty.");
                RuleFor(x => x.Page)
                    .GreaterThan(0).WithMessage("Page must be greater than 0.");
                RuleFor(x => x.Limit)
                    .GreaterThan(0).WithMessage("Limit must be greater than 0.");
            }
        }
    }
}