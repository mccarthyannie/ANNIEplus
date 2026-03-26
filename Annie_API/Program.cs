using Microsoft.EntityFrameworkCore;
using Annie_API.Models;
using Microsoft.AspNetCore.Identity;
using Annie_API.Data;
using Annie_API.UnitsOfWork.Interfaces;
using Annie_API.UnitsOfWork.Implementations;
using Annie_API.Repositories.Interfaces;
using Annie_API.Repositories.Implementations;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();

// disables checks for password complexity for development
// email must be unique 
builder.Services.AddIdentity<User, IdentityRole>(u =>
{ 
    u.User.RequireUniqueEmail = true;
    u.Password.RequireDigit = false;
    u.Password.RequiredUniqueChars = 0;
    u.Password.RequireLowercase = false;
    u.Password.RequireUppercase = false;
    u.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
    AddJwtBearer(x => x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
            ClockSkew = TimeSpan.Zero // Disable clock skew to ensure tokens are valid immediately        
        });

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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
