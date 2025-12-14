using Microsoft.EntityFrameworkCore;
using MyApi.Repositories;
using MyApi.Data;
using MyApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
//  HttpClient — naudojamas Spotify API paieškai
// ===================================================
builder.Services.AddHttpClient();

// ===================================================
//  Duomenų bazės konfigūracija (PostgreSQL per EF Core)
// ===================================================
builder.Services.AddDbContext<PlaylistAppContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

// ===================================================
//  Servisų registravimas (Dependency Injection)
// ===================================================
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICollaborativePlaylistService, CollaborativePlaylistService>();

// ===================================================
//  CORS — leidžiam frontend'ui jungtis prie API
// ===================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//     {
//         policy.WithOrigins(frontendUrl)
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//     });
// });

// ===================================================
// Controllers + JSON nustatymai
// ===================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ===================================================
// JWT Authentication
// ===================================================
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourAppName";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "YourAppName";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,              // <-- NAUDOJAME KINTAMUOSIUS
            ValidAudience = jwtAudience,          // <-- NAUDOJAME KINTAMUOSIUS
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))  // <-- NAUDOJAME KINTAMUOSIUS
        };
    });

builder.Services.AddAuthorization();

// ===================================================
//  Sukuriam WebApplication objektą
// ===================================================
var app = builder.Build();

// ===================================================
//  AUTOMATINĖS EF MIGRACIJOS
// ===================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlaylistAppContext>();
    db.Database.Migrate();
}


// ===================================================
//  Middleware pipeline (užklausų apdorojimo seka)
// ===================================================
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===================================================
// Paleidžiam programą
// ===================================================
app.Run();
public partial class Program { }