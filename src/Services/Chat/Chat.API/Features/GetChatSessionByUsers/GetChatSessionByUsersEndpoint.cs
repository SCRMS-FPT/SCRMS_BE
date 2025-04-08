using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Features.GetChatSessionByUsers
{
    public class GetChatSessionByUsersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/chats", async (
                ISender sender,
                Guid user1_id,
                Guid user2_id,
                HttpContext httpContext) =>
            {
                // Verify that user1_id matches the authenticated user's ID
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Results.Unauthorized();

                if (!Guid.TryParse(userIdClaim.Value, out var currentUserId))
                    return Results.BadRequest("Invalid user ID in token");

                // Ensure the authenticated user is user1_id for security
                if (currentUserId != user1_id)
                    return Results.Forbid();

                var query = new GetChatSessionByUsersQuery(user1_id, user2_id);
                var validator = new GetChatSessionByUsersQueryValidator();
                var validationResult = await validator.ValidateAsync(query);

                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors);
                }

                var result = await sender.Send(query);

                if (result == null)
                    return Results.NotFound();

                return Results.Ok(new
                {
                    chat_session_id = result.ChatSessionId,
                    user1_id = result.User1Id,
                    user2_id = result.User2Id,
                    created_at = result.CreatedAt,
                    updated_at = result.UpdatedAt
                });
            })
            .RequireAuthorization()
            .WithName("GetChatSessionByUsers");
        }

        public class GetChatSessionByUsersQueryValidator : AbstractValidator<GetChatSessionByUsersQuery>
        {
            public GetChatSessionByUsersQueryValidator()
            {
                RuleFor(x => x.User1Id)
                    .NotEmpty().WithMessage("User1Id cannot be empty.");
                RuleFor(x => x.User2Id)
                    .NotEmpty().WithMessage("User2Id cannot be empty.");
            }
        }
    }
}