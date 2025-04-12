using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Payment.API.Features.CoachRevenueReport
{
    public class GetCoachRevenueReportEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coach/reports/revenue", async (
                ISender sender,
                HttpContext httpContext,
                string? start_date,
                string? end_date,
                string? select_by = "month") =>
            {
                // Extract coach ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                var query = new GetCoachRevenueReportQuery(coachId, start_date, end_date, select_by ?? "month");
                var report = await sender.Send(query);
                return Results.Ok(report);
            })
            .RequireAuthorization("Coach")
            .WithName("GetCoachRevenueReport")
            .WithTags("Coach Reports")
            .Produces<CoachRevenueReportDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithDescription("Get revenue report for the authenticated coach with options to filter by date range and group by month, quarter, or year.");
        }
    }
}