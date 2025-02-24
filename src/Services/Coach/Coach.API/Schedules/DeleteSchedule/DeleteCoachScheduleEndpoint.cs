﻿using System.IdentityModel.Tokens.Jwt;

namespace Coach.API.Schedules.DeleteSchedule
{
    public class DeleteScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/schedules/{scheduleId:guid}", async (
                Guid scheduleId,
                ISender sender,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachUserId))
                    return Results.Unauthorized();

                var command = new DeleteScheduleCommand(scheduleId, coachUserId);
                var result = await sender.Send(command);

                return result.IsDeleted ? Results.NoContent() : Results.Problem("Failed to delete schedule.");
            })
            .RequireAuthorization()
            .WithName("DeleteCoachSchedule")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithSummary("Delete a Coach's Schedule")
            .WithDescription("Deletes a schedule for a coach if no bookings are associated.");
        }
    }
    }
