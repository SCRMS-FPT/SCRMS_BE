using Identity.API;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký các dịch vụ từ các tầng
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Cấu hình pipeline
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync(); 
}

app.Run();