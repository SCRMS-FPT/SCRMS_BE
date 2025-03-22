using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Dashboard.GetStats
{
    public class GetStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Endpoint yêu cầu người dùng có quyền 'coach' hoặc 'admin'
            app.MapGet("/api/coach/dashboard/stats", async (
                [FromQuery] string? start_date,
                [FromQuery] string? end_date,
                [FromQuery] string group_by, // move group_by after other parameters
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                // Parse start_date
                DateOnly? startDate = string.IsNullOrEmpty(start_date) ? null : DateOnly.TryParse(start_date, out var parsedStart) ? parsedStart : throw new ArgumentException("Invalid start_date format. Use YYYY-MM-DD.");

                // Parse end_date
                DateOnly? endDate = string.IsNullOrEmpty(end_date) ? null : DateOnly.TryParse(end_date, out var parsedEnd) ? parsedEnd : throw new ArgumentException("Invalid end_date format. Use YYYY-MM-DD.");

                // Kiểm tra start_date <= end_date
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                    return Results.BadRequest("start_date must be less than or equal to end_date.");

                // Kiểm tra group_by hợp lệ
                var validGroupBy = new[] { "day", "week", "month", "year" };
                if (!validGroupBy.Contains(group_by.ToLower()))
                    return Results.BadRequest("Invalid group_by value. Use 'day', 'week', 'month', or 'year'.");

                // Lấy coachId từ JWT token (cho phép cả coach và admin)
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                    ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // Tạo command
                var command = new GetStatsCommand(coachId, startDate, endDate, group_by.ToLower());
                var result = await sender.Send(command);

                return Results.Ok(result);
            })
            // Thêm RequireAuthorization để yêu cầu người dùng có quyền "coach" hoặc "admin"
            .RequireAuthorization(policy => policy
                .RequireRole("Coach", "Admin")) // Chỉ cho phép người có vai trò "coach" hoặc "admin" truy cập
            .WithName("GetCoachDashboardStats")
            .Produces<GetStatsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Dashboard Stats")
            .WithDescription("Lấy số liệu tổng hợp cho dashboard của coach.").WithTags("Dashboad");
        }
    }
}