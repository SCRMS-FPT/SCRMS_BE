using BuildingBlocks.Pagination;
using CourtBooking.Application.CourtManagement.Command.CreateCourt;
using CourtBooking.Application.CourtManagement.Command.UpdateCourt;
using CourtBooking.Application.CourtManagement.Command.DeleteCourt;
using CourtBooking.Application.CourtManagement.Queries.GetCourts;
using CourtBooking.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Endpoints
{
    public record CreateCourtRequest(CourtCreateDTO Court);
    public record CreateCourtResponse(Guid Id);
    public record GetCourtDetailsResponse(CourtDTO Court);
    public record GetCourtsResponse(PaginatedResult<CourtDTO> Courts);
    public record GetAllCourtsOfSportCenterResponse(List<CourtDTO> Courts);
    public record UpdateCourtRequest(CourtUpdateDTO Court);
    public record UpdateCourtResponse(bool IsSuccess);
    public record DeleteCourtResponse(bool IsSuccess);

    public class CourtEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/courts").WithTags("Court");

            // Create Court
            group.MapPost("/", [Authorize(Policy = "CourtOwner")] async ([FromBody] CreateCourtRequest request, ISender sender) =>
            {
                var command = new CreateCourtCommand(request.Court);
                var result = await sender.Send(command);
                var response = new CreateCourtResponse(result.Id);
                return Results.Created($"/api/courts/{response.Id}", response);
            })
            .WithName("CreateCourt")
            .Produces<CreateCourtResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Tạo sân mới")
            .WithDescription("Tạo một sân mới (yêu cầu quyền CourtOwner)");

            // Get Court Details
            group.MapGet("/{id:guid}", [Authorize] async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCourtDetailsQuery(id));
                var response = new GetCourtDetailsResponse(result.Court);
                return Results.Ok(response);
            })
            .WithName("GetCourtDetails")
            .Produces<GetCourtDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy chi tiết sân")
            .WithDescription("Lấy thông tin chi tiết của một sân cụ thể theo ID");

            // Get Courts
            group.MapGet("/", [Authorize] async ([AsParameters] PaginationRequest request,
                [FromQuery] Guid? sportCenterId, [FromQuery] Guid? sportId, [FromQuery] string? courtType, ISender sender) =>
            {
                var query = new GetCourtsQuery(request, sportCenterId, sportId, courtType);
                var result = await sender.Send(query);
                var response = new GetCourtsResponse(result.Courts);
                return Results.Ok(response);
            })
            .WithName("GetCourts")
            .Produces<GetCourtsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Lấy danh sách sân")
            .WithDescription("Lấy danh sách sân có phân trang và lọc theo trung tâm, môn thể thao và loại sân");

            // Update Court
            group.MapPut("/{id:guid}", [Authorize(Policy = "CourtOwnerOfCenter")] async (Guid id, [FromBody] UpdateCourtRequest request, ISender sender) =>
            {
                var command = new UpdateCourtCommand(id, request.Court);
                var result = await sender.Send(command);
                var response = new UpdateCourtResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("UpdateCourt")
            .Produces<UpdateCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .WithSummary("Cập nhật sân")
            .WithDescription("Cập nhật thông tin sân (yêu cầu quyền sở hữu)");

            // Delete Court
            group.MapDelete("/{id:guid}", [Authorize(Policy = "CourtOwnerOfCenter")] async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteCourtCommand(id));
                var response = new DeleteCourtResponse(result.IsSuccess);
                return Results.Ok(response);
            })
            .WithName("DeleteCourt")
            .Produces<DeleteCourtResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Xóa sân")
            .WithDescription("Xóa một sân cụ thể theo ID (yêu cầu quyền sở hữu)");
        }
    }
}