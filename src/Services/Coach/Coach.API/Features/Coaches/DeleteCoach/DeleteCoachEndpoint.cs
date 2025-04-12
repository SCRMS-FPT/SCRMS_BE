using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Coaches.DeleteCoach
{
    public class DeleteCoachEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/coaches/{coachId:guid}", async (
                Guid coachId,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                // Xác thực: Chỉ Admin mới có thể xóa coach
                if (!httpContext.User.IsInRole("Admin"))
                {
                    return Results.Forbid();
                }

                var command = new DeleteCoachCommand(coachId);
                var result = await sender.Send(command);

                return Results.Ok(new { success = result, message = "Coach has been deleted successfully" });
            })
            .RequireAuthorization("Admin")
            .WithName("DeleteCoach")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Coach")
            .WithDescription("Soft delete a coach by changing their status to 'deleted'").WithTags("Coach");
        }
    }
}