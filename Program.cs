using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TheStoreAPI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Store API",
        Version = "v1",
        Description = "API for managing The Store backend"
    });
});

builder.Services.AddDbContext<TheStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Enable Swagger always
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Store API v1");
    c.RoutePrefix = string.Empty; // this makes Swagger UI load at "/"
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
