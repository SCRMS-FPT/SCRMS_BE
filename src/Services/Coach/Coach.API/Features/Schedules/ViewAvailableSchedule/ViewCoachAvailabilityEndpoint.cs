using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Schedules.GetCoachSchedules
{
    public class GetCoachSchedulesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coach-schedules", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                string start_date,
                string end_date,
                int page = 1,
                int recordPerPage = 10) =>
            {
                // Kiểm tra và parse start_date
                if (!DateOnly.TryParse(start_date, out var startDate))
                    return Results.BadRequest("Invalid start_date format. Use YYYY-MM-DD.");

                // Kiểm tra và parse end_date
                if (!DateOnly.TryParse(end_date, out var endDate))
                    return Results.BadRequest("Invalid end_date format. Use YYYY-MM-DD.");

                // Kiểm tra start_date <= end_date
                if (startDate > endDate)
                    return Results.BadRequest("start_date must be less than or equal to end_date.");

                // Lấy coachId từ JWT token
                var coachIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (coachIdClaim == null || !Guid.TryParse(coachIdClaim.Value, out var coachId))
                    return Results.Unauthorized();

                // Tạo query
                var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, page, recordPerPage);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .RequireAuthorization("Coach")
            .WithName("GetCoachSchedules")
            .Produces<CoachSchedulesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get Coach Schedules")
            .WithDescription("Lấy danh sách lịch làm việc của coach theo ngày với phân trang và lọc.").WithTags("Schedule");

            app.MapGet("/api/public/coach-schedules/{coachId:guid}", async (
               [FromServices] ISender sender,
               Guid coachId,
               string start_date,
               string end_date,
               int page = 1,
               int recordPerPage = 10) =>
           {
               // Validate and parse start_date
               if (!DateOnly.TryParse(start_date, out var startDate))
                   return Results.BadRequest("Invalid start_date format. Use YYYY-MM-DD.");

               // Validate and parse end_date
               if (!DateOnly.TryParse(end_date, out var endDate))
                   return Results.BadRequest("Invalid end_date format. Use YYYY-MM-DD.");

               // Verify start_date <= end_date
               if (startDate > endDate)
                   return Results.BadRequest("start_date must be less than or equal to end_date.");

               // Create query with provided coachId
               var query = new GetCoachSchedulesQuery(coachId, startDate, endDate, page, recordPerPage);
               var result = await sender.Send(query);

               return Results.Ok(result);
           })
           .WithName("GetPublicCoachSchedules")
           .Produces<CoachSchedulesResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithSummary("Get Public Coach Schedules")
           .WithDescription("Lấy danh sách lịch làm việc của coach theo ngày với phân trang và lọc, không yêu cầu đăng nhập.")
           .WithTags("Public", "Schedule");
        }
    }
}