using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Packages.GetHistoryPurchase
{
    public class GetPurchaseDetailEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coach-packages/purchases", async (
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] bool? IsExpiried,
                [FromQuery] bool? IsOutOfUse,
                [FromQuery] Guid? CoachId,
                [FromQuery] int Page = 1,
                [FromQuery] int RecordPerPage = 10) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var query = new GetHistroryPurchaseQuery(
                    userId,
                    Page,
                    RecordPerPage,
                    IsExpiried,
                    IsOutOfUse,
                    CoachId
                );
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetHistoryPurchase")
            .Produces<List<PurchaseRecord>>(StatusCodes.Status200OK).WithTags("Package");
        }
    }
}