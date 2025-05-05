using Coach.API.Data;
using Coach.API.Services;
using Coach.API.Consumers;
using Coach.API.Data.Repositories;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using MassTransit;
using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Extensions;
using Coach.API.Data.Extensions;
using Coach.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddScoped<ICoachRepository, CoachRepository>();
builder.Services.AddScoped<ICoachScheduleRepository, CoachScheduleRepository>();
builder.Services.AddScoped<ICoachBookingRepository, CoachBookingRepository>();
builder.Services.AddScoped<ICoachPackageRepository, CoachPackageRepository>();
builder.Services.AddScoped<ICoachSportRepository, CoachSportRepository>();
builder.Services.AddScoped<ICoachPromotionRepository, CoachPromotionRepository>();
builder.Services.AddScoped<ICoachPackagePurchaseRepository, CoachPackagePurchaseRepository>();
// Add these configurations to your service registration
builder.Services.Configure<ImageKitOptions>(builder.Configuration.GetSection("ImageKit"));
builder.Services.AddHttpClient<IImageKitService, ImageKitService>();

// Chỉ đăng ký MessageBroker đơn giản
builder.Services.AddMessageBroker(builder.Configuration, assembly);

// Add outbox services
builder.Services.AddOutboxServices();

builder.Services.AddCarter();
builder.Services.AddCors();
builder.Services.AddDbContext<CoachDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "identity-service",
        ValidAudience = "webapp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("8f9c08c9e6bde3fc8697fbbf91d52a5dcd2f72f84b4b8a6c7d8f3f9d3db249a1")),
        RoleClaimType = ClaimTypes.Role
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("Coach", policy => policy.RequireRole("Coach"));
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Cấu hình Bearer Token cho Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Please enter token in the format: Bearer {your_token_here}"
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

// Thêm Outbox pattern
//builder.Services.AddOutbox<CoachDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapCarter();
app.UseCors(builder =>
{
    builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});
app.UseExceptionHandler(options => { });
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();