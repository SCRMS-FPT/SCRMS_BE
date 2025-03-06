using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Provider.Consul;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Load config theo môi trường
builder.Configuration
    .AddJsonFile(
        $"ocelot.{(builder.Environment.IsProduction() ? "Production" : "Development")}.json",
        optional: false,
        reloadOnChange: true
    );

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("WWW-Authenticate");
    });
});

// Cấu hình JWT Authentication
builder.Services.AddAuthentication("IdentityAuthKey")
    .AddJwtBearer("IdentityAuthKey", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "identity-service",
            ValidateAudience = true,
            ValidAudience = "webapp",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("8f9c08c9e6bde3fc8697fbbf91d52a5dcd2f72f84b4b8a6c7d8f3f9d3db249a1")
            ),
            RoleClaimType = "role",
            NameClaimType = "sub",
        };
    });

// Cấu hình Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});

// Ocelot + Consul
builder.Services.AddOcelot()
               .AddConsul();

var app = builder.Build();

// Middleware Order Fix
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Log headers
    Console.WriteLine("Headers:");
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }

    // Log claims
    if (context.User.Identity is ClaimsIdentity identity)
    {
        Console.WriteLine("\nClaims:");
        foreach (var claim in identity.Claims)
        {
            Console.WriteLine($"{claim.Type} => {claim.Value}");
        }
    }

    await next();
});

await app.UseOcelot();
app.Run();