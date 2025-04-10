using Microsoft.AspNetCore.Mvc;

namespace Coach.API.Features.Coaches.RestoreCoach
{
    public class RestoreCoachEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/admin/coaches/{coachId:guid}/restore", async (
                Guid coachId,
                [FromServices] ISender sender) =>
            {
                var command = new RestoreCoachCommand(coachId);
                var result = await sender.Send(command);

                return Results.Ok(new { success = result, message = "Coach has been restored successfully" });
            })
            .RequireAuthorization("Admin")
            .WithName("RestoreCoach")
            .Produces<object>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Restore Coach")
            .WithDescription("Restore a previously deleted coach by setting their status back to 'active'").WithTags("Admin");
        }
    }
}