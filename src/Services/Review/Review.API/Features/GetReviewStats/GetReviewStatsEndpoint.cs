using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Reviews.API.Features.GetReviewStats
{
    public class GetReviewStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/reviews/stats",
                [Authorize(Roles = "Admin")] async (
                    [FromQuery] DateTime? start_date,
                    [FromQuery] DateTime? end_date,
                    IMediator mediator,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetReviewStatsQuery
                    {
                        StartDate = start_date,
                        EndDate = end_date
                    };

                    var result = await mediator.Send(query, cancellationToken);

                    // Chuyển đổi kết quả sang snake_case theo yêu cầu
                    return Results.Ok(new
                    {
                        total_reviews = result.TotalReviews,
                        reported_reviews = result.ReportedReviews,
                        date_range = new
                        {
                            start_date = result.DateRange.StartDate,
                            end_date = result.DateRange.EndDate
                        }
                    });
                })
                .WithName("GetReviewStats")
                .WithTags("Admin")
                .Produces<object>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status403Forbidden);
        }
    }
}