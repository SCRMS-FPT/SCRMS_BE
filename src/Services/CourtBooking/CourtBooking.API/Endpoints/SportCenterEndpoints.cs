using BuildingBlocks.Pagination;
using CourtBooking.Application.SportCenterManagement.Commands.CreateSportCenter;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenters;
using CourtBooking.Application.CourtManagement.Queries.GetSportCenterById;
using CourtBooking.Application.CourtManagement.Command.UpdateSportCenter;
using CourtBooking.Application.CourtManagement.Command.DeleteSportCenter;
using CourtBooking.Application.CourtManagement.Command.SoftDeleteSportCenter;
using CourtBooking.Application.CourtManagement.Command.RestoreSportCenter;
using Microsoft.AspNetCore.Authorization;
using CourtBooking.Application.CourtManagement.Queries.GetAllCourtsOfSportCenter;
using CourtBooking.Application.CourtManagement.Queries.GetSportCentersByOwner;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using CourtBooking.Infrastructure.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace CourtBooking.API.Endpoints
{
    public record CreateSportCenterRequest(CreateSportCenterCommand SportCenter);
    public record CreateSportCenterResponse(Guid Id);
    public record GetOwnedSportCentersResponse(PaginatedResult<SportCenterListDTO> SportCenters);
    public record GetSportCentersResponse(PaginatedResult<SportCenterListDTO> SportCenters);
    public record UpdateSportCenterRequest(
        Guid SportCenterId,
        string Name,
        string PhoneNumber,
        string Description,
        LocationDTO Location,
        GeoLocation LocationPoint,
        SportCenterImages Images
    );
    public record UpdateSportCenterResponse(bool Success);

    public class SportCenterEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/sportcenters").WithTags("SportCenter");

            // Create Sport Center
            group.MapPost("/", async ([FromForm] CreateSportCenterFormModel model,
                                    [FromServices] IFileStorageService fileStorage,
                                    [FromServices] ISender sender,
                                    HttpContext httpContext) =>
            {
                var form = await httpContext.Request.ReadFormAsync();
                // Upload the avatar image
                string avatarUrl = "";
                if (model.AvatarImage != null)
                {
                    avatarUrl = await fileStorage.UploadFileAsync(model.AvatarImage, "sportcenters/avatars");
                }

                // Upload gallery images
                List<string> galleryUrls = new List<string>();
                var GalleryImages = model.GalleryImages ?? form.Files.GetFiles("GalleryImages").ToList() ?? new List<IFormFile>();
                if (GalleryImages != null && GalleryImages.Count > 0)
                {
                    galleryUrls = await fileStorage.UploadFilesAsync(GalleryImages, "sportcenters/gallery");
                }
                // Create the command with the uploaded image URLs
                var command = new CreateSportCenterCommand(
                    Name: model.Name,
                    PhoneNumber: model.PhoneNumber,
                    AddressLine: model.AddressLine,
                    City: model.City,
                    District: model.District,
                    Commune: model.Commune,
                    Latitude: model.Latitude,
                    Longitude: model.Longitude,
                    Avatar: avatarUrl,
                    ImageUrls: galleryUrls,
                    Description: model.Description
                );

                var result = await sender.Send(command);
                var response = new CreateSportCenterResponse(result.Id);
                return Results.Created($"/api/sportcenters/{response.Id}", response);
            })
           .WithName("CreateSportCenter")
           .RequireAuthorization("AdminOrCourtOwner")
           .DisableAntiforgery()
           .Accepts<CreateSportCenterFormModel>("multipart/form-data") // Accept multipart/form-data
           .Produces<CreateSportCenterResponse>(StatusCodes.Status201Created)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithSummary("Create Sport Center with Images")
           .WithDescription("Create a new sport center with uploaded images");

            // Get Sport Centers
            group.MapGet("/", async (
                [FromQuery] int? page, // 1-based
                [FromQuery] int? limit,
                [FromQuery] string? city,
                [FromQuery] string? name,
                [FromQuery] Guid? SportId,
                [FromQuery] DateTime? BookingDate,
                [FromQuery] TimeSpan? StartTime,
                [FromQuery] TimeSpan? EndTime,
                HttpContext httpContext,
                ISender sender) =>
            {
                var paginationRequest = new PaginationRequest((page ?? 1) - 1, limit ?? 10);

                // Kiểm tra role của người dùng
                Guid? excludeOwnerId = null;
                var user = httpContext.User;

                // Nếu người dùng đã đăng nhập
                if (user.Identity?.IsAuthenticated == true)
                {
                    // Kiểm tra xem người dùng có role CourtOwner không
                    if (user.IsInRole("CourtOwner"))
                    {
                        // Trích xuất userId từ claims
                        var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                         user.FindFirst(ClaimTypes.NameIdentifier);

                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var ownerId))
                        {
                            // Nếu là CourtOwner, loại bỏ các sportcenter của họ
                            excludeOwnerId = ownerId;
                        }
                    }
                }

                var query = new GetSportCentersQuery(
                    paginationRequest,
                    city,
                    name,
                    SportId,
                    BookingDate,
                    StartTime,
                    EndTime,
                    excludeOwnerId);

                var result = await sender.Send(query);
                var response = result.Adapt<GetSportCentersResponse>();
                return Results.Ok(response);
            })
            .WithName("GetSportCenters")
            .Produces<GetSportCentersResponse>(StatusCodes.Status200OK)
            .WithSummary("Get Sport Centers")
            .WithDescription("Get a paginated list of sport centers with optional filters");

            // Get All Courts of Sport Center
            group.MapGet("/{id:guid}/courts", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetAllCourtsOfSportCenterQuery(id));
                var response = new GetAllCourtsOfSportCenterResponse(result.Courts);
                return Results.Ok(response);
            })
            .WithName("GetAllCourtsOfSportCenter")
            .Produces<GetAllCourtsOfSportCenterResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Lấy tất cả sân của một trung tâm")
            .WithDescription("Lấy tất cả sân của một trung tâm thể thao cụ thể theo ID");
            group.MapGet("/owned", async (
                            HttpContext httpContext,
                            [FromQuery] int? page, // 1-based
                            [FromQuery] int? limit,
                            [FromQuery] string? city,
                            [FromQuery] string? name,
                            [FromQuery] Guid? SportId,
                            [FromQuery] DateTime? BookingDate,
                            [FromQuery] TimeSpan? StartTime,
                            [FromQuery] TimeSpan? EndTime,
                            ISender sender) =>
                        {
                            // Extract user ID from JWT token
                            var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                                                         ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var ownerId))
                            {
                                return Results.Unauthorized();
                            }

                            var paginationRequest = new PaginationRequest((page ?? 1) - 1, limit ?? 10);
                            var query = new GetSportCentersByOwnerQuery(
                                ownerId,
                                paginationRequest,
                                city,
                                name,
                                SportId,
                                BookingDate,
                                StartTime,
                                EndTime);

                            var result = await sender.Send(query);
                            var response = new GetOwnedSportCentersResponse(result.SportCenters);
                            return Results.Ok(response);
                        })
                        .WithName("GetOwnedSportCenters")
                        .RequireAuthorization("CourtOwner")
                        .Produces<GetOwnedSportCentersResponse>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .ProducesProblem(StatusCodes.Status403Forbidden)
                        .WithSummary("Get Sport Centers by Owner")
                        .WithDescription("Get all sport centers owned by the authenticated Court Owner with optional filters");

            // Update Sport Center
            group.MapPut("/{centerId:guid}", async (
                Guid centerId,
                [FromForm] UpdateSportCenterFormModel model,
                [FromServices] IFileStorageService fileStorage,
                [FromServices] ISender sender,
                HttpContext httpContext) =>
            {
                var form = await httpContext.Request.ReadFormAsync();
                // Handle avatar image
                string avatarUrl = model.ExistingAvatar ?? "";

                if (!model.KeepExistingAvatar || model.AvatarImage != null)
                {
                    // If not keeping existing avatar or uploading a new one
                    if (model.AvatarImage != null)
                    {
                        // Upload new avatar
                        avatarUrl = await fileStorage.UploadFileAsync(model.AvatarImage, "sportcenters/avatars");
                    }
                    else
                    {
                        // User wants to remove avatar without providing a new one
                        avatarUrl = "";
                    }
                }

                // Handle gallery images
                List<string> galleryUrls = new List<string>();
                if (model.KeepExistingGallery && model.ExistingGalleryUrls != null)
                {
                    galleryUrls.AddRange(model.ExistingGalleryUrls);
                }
                var GalleryImages = model.GalleryImages ?? form.Files.GetFiles("GalleryImages").ToList() ?? new List<IFormFile>();
                if (GalleryImages != null && GalleryImages.Count > 0)
                {
                    // Add new gallery images
                    var newGalleryUrls = await fileStorage.UploadFilesAsync(GalleryImages, "sportcenters/gallery");
                    galleryUrls.AddRange(newGalleryUrls);
                }

                // Create command with updated information
                var command = new UpdateSportCenterCommand(
                    SportCenterId: centerId,
                    Name: model.Name,
                    PhoneNumber: model.PhoneNumber,
                    AddressLine: model.AddressLine,
                    City: model.City,
                    District: model.District,
                    Commune: model.Commune,
                    Latitude: model.Latitude,
                    Longitude: model.Longitude,
                    Avatar: avatarUrl,
                    ImageUrls: galleryUrls,
                    Description: model.Description
                );

                var result = await sender.Send(command);
                return Results.Ok(result.SportCenter);
            })
            .WithName("UpdateSportCenter")
            .RequireAuthorization("AdminOrCourtOwner")
            .DisableAntiforgery() // Disable CSRF for file uploads
            .Accepts<UpdateSportCenterFormModel>("multipart/form-data")
            .Produces<SportCenterDetailDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Sport Center with Images")
            .WithDescription("Updates the information of an existing sport center with optional image uploads");

            group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetSportCenterByIdQuery(id);
                var result = await sender.Send(query);
                return Results.Ok(result.SportCenter);
            })
            .WithName("GetSportCenterById")
            //.RequireAuthorization() // Yêu cầu JWT, cho phép mọi user đã đăng nhập
            .Produces<SportCenterListDTO>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get Sport Center By ID")
            .WithDescription("Get detailed information of a specific sport center");

            // Hard Delete Sport Center
            group.MapDelete("/{centerId:guid}", async (
                Guid centerId,
                ISender sender) =>
            {
                var command = new DeleteSportCenterCommand(centerId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("DeleteSportCenter")
            .RequireAuthorization("AdminOrCourtOwner")
            .Produces<DeleteSportCenterResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete Sport Center")
            .WithDescription("Permanently deletes a sport center and all its courts");

            // Soft Delete Sport Center
            group.MapDelete("/{centerId:guid}/soft", async (
                Guid centerId,
                ISender sender) =>
            {
                var command = new SoftDeleteSportCenterCommand(centerId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("SoftDeleteSportCenter")
            .RequireAuthorization("AdminOrCourtOwner")
            .Produces<SoftDeleteSportCenterResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Soft Delete Sport Center")
            .WithDescription("Marks a sport center as deleted and sets all its courts to Closed status");

            // Restore Sport Center
            group.MapPost("/{centerId:guid}/restore", async (
                Guid centerId,
                ISender sender) =>
            {
                var command = new RestoreSportCenterCommand(centerId);
                var result = await sender.Send(command);

                if (result.Success)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            })
            .WithName("RestoreSportCenter")
            .RequireAuthorization("AdminOrCourtOwner")
            .Produces<RestoreSportCenterResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Restore Sport Center")
            .WithDescription("Restores a previously soft-deleted sport center and sets available courts back to Open status");
        }
    }

    public class CreateSportCenterFormModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Commune { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public IFormFile AvatarImage { get; set; }
        public List<IFormFile> GalleryImages { get; set; }
    }

    public class UpdateSportCenterFormModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Commune { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public IFormFile? AvatarImage { get; set; }
        public List<IFormFile>? GalleryImages { get; set; }
        public string? ExistingAvatar { get; set; }
        public List<string>? ExistingGalleryUrls { get; set; }
        public bool KeepExistingAvatar { get; set; } = true;
        public bool KeepExistingGallery { get; set; } = true;
    }
}