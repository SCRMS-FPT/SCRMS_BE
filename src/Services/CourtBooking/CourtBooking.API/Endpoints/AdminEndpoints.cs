using CourtBooking.Application.CourtManagement.Queries.GetCourtStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record GetCourtStatsResponse(long total_courts, decimal total_courts_revenue, object date_range);

    public class AdminEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/admin").WithTags("Admin");

            // Thống kê sân và doanh thu
            group.MapGet("/court-stats", [Authorize(Roles = "Admin")] async (
                [FromQuery] DateTime? start_date,
                [FromQuery] DateTime? end_date,
                ISender sender) =>
            {
                var query = new GetCourtStatsQuery(start_date, end_date);
                var result = await sender.Send(query);
                
                // Chuyển đổi định dạng để phù hợp với snake_case theo yêu cầu
                var response = new GetCourtStatsResponse(
                    result.TotalCourts,
                    result.TotalCourtsRevenue,
                    new 
                    { 
                        start_date = result.DateRange.StartDate, 
                        end_date = result.DateRange.EndDate 
                    }
                );
                
                return Results.Ok(response);
            })
            .WithName("GetCourtStats")
            .Produces<GetCourtStatsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Thống kê sân và doanh thu")
            .WithDescription("Trả về số liệu tổng hợp của các sân và doanh thu trong khoảng thời gian (yêu cầu quyền Admin)");

        }
    }
} 