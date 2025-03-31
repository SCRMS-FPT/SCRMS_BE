using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Coach.API.Features.Promotion.GetAllPromotion
{
    public class GetAllPromotionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/coaches/{coachId:guid}/promotions", async (
                Guid coachId,
                [FromServices] ISender sender,
                HttpContext httpContext,
                [FromQuery] int Page = 1,
                [FromQuery] int RecordPerPage = 10) =>
            {

                var command = new GetAllPromotionQuery(
                    coachId,
                    Page,
                    RecordPerPage
                );
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("GetAllPromotion")
            .Produces<List<PromotionRecord>>(StatusCodes.Status200OK).WithTags("Promotion");
        }
    }
}