using BuildingBlocks.Pagination;
using Identity.Application.Identity.Commands.UserManagement;
using Identity.Application.Identity.Queries.UserManagement;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Endpoints
{
    public class UserManagementEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var userManagementGroup = app.MapGroup("/api/users")
                                         .WithTags("User Management");

            userManagementGroup.MapDelete("/{userId}", async (Guid userId, ISender sender) =>
            {
                await sender.Send(new DeleteUserCommand(userId));
                return Results.NoContent();
            }).RequireAuthorization("Admin");

            userManagementGroup.MapGet("/", async (
                ISender sender,
                [FromQuery] string? searchTerm,
                [FromQuery] string? role,
                [FromQuery] int pageIndex = 0,
                [FromQuery] int pageSize = 10) =>
            {
                var pagination = new PaginationRequest(pageIndex, pageSize);
                var query = new GetUsersQuery(searchTerm, role, pagination);
                var result = await sender.Send(query);
                return Results.Ok(result);
            }).RequireAuthorization("Admin");

            userManagementGroup.MapGet("/{id}/full", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetUserByIdQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            }).RequireAuthorization("Admin");

            userManagementGroup.MapGet("/{id}/profile", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetUserProfileByIdQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            userManagementGroup.MapPut("/{id}", async (Guid id, UpdateUserRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateUserCommand>() with { UserId = id };
                var result = await sender.Send(command);
                return Results.Ok(result);
            }).RequireAuthorization("Admin");
        }

        public record UpdateUserRequest(
           string FirstName,
           string LastName,
           DateTime BirthDate,
           string Gender,
           string Phone,
           string SelfIntroduction
       );
    }
}