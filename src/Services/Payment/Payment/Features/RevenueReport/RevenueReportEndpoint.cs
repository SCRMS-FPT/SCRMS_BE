namespace Payment.API.Features.RevenueReport
{
    public class GetRevenueReportEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/reports/revenue", async (ISender sender, HttpContext httpContext, string? start_date, string? end_date) =>
            {
                var query = new GetRevenueReportQuery(start_date, end_date);
                var report = await sender.Send(query);
                return Results.Ok(report);
            })
            .RequireAuthorization("Admin")
            .WithName("GetRevenueReport");
        }
    }
}