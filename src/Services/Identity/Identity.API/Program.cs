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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Cấu hình pipeline
app.UseApiServices();
app.UseAuthentication();
//app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync(); 
}


app.Run();