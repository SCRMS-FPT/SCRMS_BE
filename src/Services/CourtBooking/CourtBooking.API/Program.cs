using CourtBooking.API;
using CourtBooking.Application;
using CourtBooking.Infrastructure;
using CourtBooking.Infrastructure.Data.Extensions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CourtBooking API", Version = "v1" });

    // Cấu hình Bearer Token cho Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Vui lòng nhập token theo định dạng: Bearer {your_token_here}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>

options.AddPolicy("ReactCORS", policy =>
{
    policy.WithOrigins("http://localhost:5173", "http://localhost:5174") // Chỉ định rõ origin
          .AllowAnyMethod()
          .AllowAnyHeader()
          .SetIsOriginAllowed(origin => true)
          .SetIsOriginAllowedToAllowWildcardSubdomains()
          .AllowCredentials(); // Bắt buộc cho cookie
})
    );

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHttpClient("NotificationAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7069");
});

var app = builder.Build();

// Sử dụng middleware
app.UseCors("ReactCORS");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseApiServices();

app.Run();

public partial class Program
{ }