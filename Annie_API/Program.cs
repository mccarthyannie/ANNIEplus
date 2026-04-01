using Annie_API.Controllers;
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
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.EnableRetryOnFailure()
        ));


builder.Services.AddCors(options =>
{
    options.AddPolicy("MethodPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5031", "https://localhost:5031")
               .AllowAnyMethod() 
               .AllowAnyHeader();
    });
});


// transient used because seeding database will only be done when no users are present
builder.Services.AddTransient<SeedData>();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersUnitOfWork, UsersUnitOfWork>();

builder.Services.AddScoped<IEmailComposer, EmailComposer>();

// disables checks for password complexity for development
// email must be unique 
builder.Services.AddIdentity<User, IdentityRole>(u =>
{
    u.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    u.SignIn.RequireConfirmedEmail = true;
    u.User.RequireUniqueEmail = true;
    u.Password.RequireDigit = false;
    u.Password.RequiredUniqueChars = 0;
    u.Password.RequireLowercase = false;
    u.Password.RequireUppercase = false;
    u.Password.RequireNonAlphanumeric = false;
    
    // block sign in after failed attempts 
    u.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    u.Lockout.MaxFailedAccessAttempts = 5;
    u.Lockout.AllowedForNewUsers = true;
    
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

        // Used to check that swagger was sending the correct authorization header
        /*x.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                Console.WriteLine($"[JwtEvents] OnMessageReceived. Authorization header: '{ctx.Request.Headers["Authorization"]}'");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                var claims = string.Join(", ", ctx.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>());
                Console.WriteLine($"[JwtEvents] OnTokenValidated. Claims: {claims}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("[JwtEvents] OnAuthenticationFailed. Exception: " + ctx.Exception?.ToString());
                return Task.CompletedTask;
            }
        };*/
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


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    await next();
});

app.UseRouting();

app.UseCors("MethodPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
