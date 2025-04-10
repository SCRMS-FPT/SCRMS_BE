using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Reviews.API.Features.UpdateReviewFlagStatus
{
    public class UpdateReviewFlagStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/admin/reviews/flags/{flagId}", async (
                ISender sender,
                HttpContext httpContext,
                Guid flagId,
                [FromBody] UpdateReviewFlagStatusRequest request) =>
            {

                var command = new UpdateReviewFlagStatusCommand(
                    flagId,
                    request.Status,
                    request.AdminNote);

                await sender.Send(command);
                return Results.Ok(new { message = "Flag status updated successfully" });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithName("UpdateReviewFlagStatus");
        }
    }

    public class UpdateReviewFlagStatusRequest
    {
        [Required]
        [RegularExpression("^(resolved|rejected)$", ErrorMessage = "Status must be either 'resolved' or 'rejected'")]
        public string Status { get; set; }

        public string? AdminNote { get; set; }
    }
}