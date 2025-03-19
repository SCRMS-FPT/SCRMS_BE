﻿using Identity.Application.Identity.Commands.UserManagement;
using Identity.Application.Identity.Queries.UserManagement;
using Identity.Application.ServicePackages.Commands.CancelSubscription;
using Identity.Application.ServicePackages.Commands.CreatePromotion;
using Identity.Application.ServicePackages.Commands.DeletePromotion;
using Identity.Application.ServicePackages.Commands.RenewSubscription;
using Identity.Application.ServicePackages.Commands.ServicePackagesManagement;
using Identity.Application.ServicePackages.Commands.SubscribeToServicePackage;
using Identity.Application.ServicePackages.Commands.UpdatePromotion;
using Identity.Application.ServicePackages.Queries.GetPromotions;
using Identity.Application.ServicePackages.Queries.GetServicePackages;
using Identity.Application.ServicePackages.Queries.ServicePackagesManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Identity.API.Endpoints
{
    public class ServicePackagesEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Group cho các endpoint Service Packages dành cho người dùng
            var servicePackagesGroup = app.MapGroup("/api/service-packages")
                                          .WithTags("Service Packages");

            servicePackagesGroup.MapGet("/", async (ISender sender) =>
            {
                var result = await sender.Send(new GetServicePackagesQuery());
                return Results.Ok(result);
            });

            servicePackagesGroup.MapGet("/{id}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetServicePackageByIdQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            servicePackagesGroup.MapPost("/subscribe", async (SubscribeRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new SubscribeToServicePackageCommand(userId, request.PackageId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            servicePackagesGroup.MapPut("/subscribe/renew", async (RenewRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new RenewSubscriptionCommand(request.SubscriptionId, userId, request.AdditionalDurationDays);
                await sender.Send(command);
                return Results.Ok();
            });

            servicePackagesGroup.MapPut("/subscribe/cancel", async (CancelRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new CancelSubscriptionCommand(request.SubscriptionId, userId);
                await sender.Send(command);
                return Results.Ok();
            });

            // Group cho các endpoint Promotions dành cho Admin
            var promotionsGroup = app.MapGroup("/api/service-packages")
                                     .WithTags("Service Packages - Promotions")
                                     .RequireAuthorization("Admin");

            promotionsGroup.MapGet("/{packageId:guid}/promotions", async (Guid packageId, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new GetPromotionsQuery(packageId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            promotionsGroup.MapPost("/{packageId:guid}/promotions", async (Guid packageId, AddNewPromotionRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new CreatePromotionCommand(packageId, request.Description, request.Type, request.Value, request.ValidFrom, request.ValidTo);
                var result = await sender.Send(command);
                return Results.Created($"/api/promotions/{result.Id}", result);
            });

            promotionsGroup.MapPut("/promotions/{promotionId:guid}", async (Guid promotionId, UpdatePromotionRequest request, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new UpdatePromotionCommand(promotionId, request.PackageId, request.Description, request.Type, request.Value, request.ValidFrom, request.ValidTo);
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            promotionsGroup.MapDelete("/promotions/{promotionId:guid}", async (Guid promotionId, ISender sender, HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
                               ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Results.Unauthorized();

                var command = new DeletePromotionCommand(promotionId);
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            // Group cho các endpoint Quản lý Service Packages dành cho Admin
            var managePackagesGroup = app.MapGroup("/api/manage-packages")
                                         .WithTags("Service Packages - Management")
                                         .RequireAuthorization("Admin");

            managePackagesGroup.MapPost("/create", async ([FromBody] CreateServicePackageRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateServicePackageCommand>();
                var result = await sender.Send(command);
                return Results.Created($"/api/service-packages/{result.Id}", result);
            });

            managePackagesGroup.MapPut("/update/{id}", async (Guid id, [FromBody] UpdateServicePackageRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateServicePackageCommand>() with { Id = id };
                var result = await sender.Send(command);
                return Results.Ok(result);
            });

            managePackagesGroup.MapDelete("/delete/{id}", async (Guid id, ISender sender) =>
            {
                await sender.Send(new DeleteServicePackageCommand(id));
                return Results.NoContent();
            });
        }
    }

    public record SubscribeRequest(Guid PackageId);
    public record RenewRequest(Guid SubscriptionId, int AdditionalDurationDays);
    public record CancelRequest(Guid SubscriptionId);
    public record AddNewPromotionRequest(Guid PackageId, string Description, string Type, decimal Value, DateTime ValidFrom, DateTime ValidTo);
    public record UpdatePromotionRequest(Guid PromotionId, Guid PackageId, string Description, string Type, decimal Value, DateTime ValidFrom, DateTime ValidTo);
    public record CreateServicePackageRequest(
        string Name,
        string Description,
        decimal Price,
        string AssociatedRole,
        string Status,
        int DurationDays
    );
    public record UpdateServicePackageRequest(
        string Name,
        string Description,
        decimal Price,
        string AssociatedRole,
        string Status,
        int DurationDays
    );
}