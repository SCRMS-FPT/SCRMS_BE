using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Payment.API.Features.CourtRevenueReport
{
    public class GetCourtRevenueReportEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/court/reports/revenue", async (
                ISender sender,
                HttpContext httpContext,
                string? start_date,
                string? end_date,
                string? select_by = "month") =>
            {
                // Extract court owner ID from JWT token
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var courtOwnerId))
                    return Results.Unauthorized();

                var query = new GetCourtRevenueReportQuery(courtOwnerId, start_date, end_date, select_by ?? "month");
                var report = await sender.Send(query);
                return Results.Ok(report);
            })
            .RequireAuthorization("CourtOwner")
            .WithName("GetCourtRevenueReport")
            .WithTags("Court Reports")
            .Produces<CourtRevenueReportDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithDescription("Get revenue report for the authenticated court owner with options to filter by date range and group by month, quarter, or year.");
        }
    }
}