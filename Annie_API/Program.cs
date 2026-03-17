using Microsoft.EntityFrameworkCore;
using Annie_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.EnableRetryOnFailure()
        ));

// transient used because seeding database will only be done when no users are present
builder.Services.AddTransient<SeedData>();

var app = builder.Build();

await SeedDb(app);

async Task SeedDb(WebApplication app) {
    var scopedFactory = app.Services.GetService<IServiceScopeFactory>();


    using (var scope = scopedFactory!.CreateScope())
    {
        var service = scope.ServiceProvider.GetService<SeedData>();
        await service.SeedAsync();
        
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
