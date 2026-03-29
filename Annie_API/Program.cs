using Annie_API.Data;
using Annie_API.Models;
using Annie_API.Repositories.Implementations;
using Annie_API.Repositories.Interfaces;
using Annie_API.UnitsOfWork.Implementations;
using Annie_API.UnitsOfWork.Interfaces;

using Annie_Shared.Auth;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Orders Backend";
        document.Info.Version = "v1";

        // Define the Bearer Security Scheme
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Name = "Authorization",
            In = ParameterLocation.Header,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token directly (no 'Bearer' prefix needed here)"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        // Apply the security requirement globally
        document.Security = [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    []
                }
            }
        ];

        return Task.CompletedTask;
    });
});

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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;

        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
            ClockSkew = TimeSpan.Zero // Disable clock skew to ensure tokens are valid immediately        
        };
    });

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("CanChangeSessions", policy => policy.Requirements.Add(new CustomRoleRequirement(new string[] { "Admin", "Instructor" })));
    config.AddPolicy("AnyValidUser", policy => policy.Requirements.Add(new CustomRoleRequirement(new string[] { "User", "Admin", "Instructor" })));
});
builder.Services.AddSingleton<IAuthorizationHandler, UserRolesHandler>();


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
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
        options.OAuthUsePkce();
    });
}

//app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
