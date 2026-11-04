using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieTicketBooking.Authentication;
using MovieTicketBooking.Data;
using MovieTicketBooking.Middleware;
using MovieTicketBooking.Repositories;
using MovieTicketBooking.Repositories.Interfaces;
using MovieTicketBooking.Services;
using MovieTicketBooking.Services.Interfaces;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT signing key is missing. Set it with User Secrets.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

byte[] jwtKeyBytes;
try { jwtKeyBytes = Convert.FromBase64String(jwtKey); }
catch (FormatException ex) { throw new InvalidOperationException("Jwt:Key must be Base64 encoded.", ex); }

if (jwtKeyBytes.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes (256 bits).");

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT authentication and authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.Name
        };
    });
builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<ITheatreRepository, TheatreRepository>();
builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IShowRepository, ShowRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Services
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ITheatreService, TheatreService>();
builder.Services.AddScoped<IScreenService, ScreenService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<IShowService, ShowService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT token"
            };

        return Task.CompletedTask;
    });
});
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ==========================================
// 1. EXCEPTION HANDLING
// ==========================================

app.UseExceptionHandler();


// ==========================================
// 2. OPENAPI AND SCALAR
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// ==========================================
// 3. CORS
// ==========================================

app.UseCors("AllowAll");


// ==========================================
// 4. HTTPS REDIRECTION
// ==========================================

app.UseHttpsRedirection();


// ==========================================
// 5. ROUTING
// ==========================================

app.UseRouting();


// ==========================================
// 6. ENDPOINT METADATA DIAGNOSTICS
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<EndpointMetadataMiddleware>();
}


// ==========================================
// 7. AUTHENTICATION
// ==========================================

// Validates JWT and loads claims into HttpContext.User

app.UseAuthentication();


// ==========================================
// 8. AUTHENTICATION DIAGNOSTICS
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<AuthenticationDiagnosticsMiddleware>();
}


// ==========================================
// 9. AUTHORIZATION DIAGNOSTICS
// ==========================================

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<AuthorizationDiagnosticsMiddleware>();
}


// ==========================================
// 10. CUSTOM AUTHORIZATION
// ==========================================

app.UseMiddleware<CustomAuthorizationMiddleware>();


// ==========================================
// 11. BUILT-IN AUTHORIZATION
// ==========================================

app.UseAuthorization();


// ==========================================
// 12. MAP CONTROLLERS
// ==========================================

app.MapControllers();


// ==========================================
// 13. DATABASE SEEDING
// ==========================================

// Apply pending EF migrations and optionally create the initial admin.

await RoleSeeder.SeedAsync(app.Services, app.Configuration);


// ==========================================
// 14. RUN APPLICATION
// ==========================================

app.Run();