using BloodDonationSystem.Data;
using BloodDonationSystem.Models;
using DomainLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Data;
using Persistence.UnitOfWork;
using Presentation;
using Service;
using ServiceAbstraction.Interfaces;
using ServiceAbstraction.Mapping;
using System.Text;
using AutoMapper;

namespace BloodDonationSystem;

public class Program
{

    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Configure the database connection string
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




        // ── ASP.NET Core Identity ─────────────────────────────────────────────
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Authentication ────────────────────────────────────────────────
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                                               Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)),
                ClockSkew = TimeSpan.Zero,
            };
        });

        builder.Services.AddAuthorization();

        // ── Application Services ──────────────────────────────────────────────
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();
        builder.Services.AddScoped<IDonationService, DonationService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IRewardService, RewardService>();
        builder.Services.AddScoped<IQrService, QrService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AutoMapper
        builder.Services.AddAutoMapper(typeof(BloodRequestProfile).Assembly);




        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Donation API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter: Bearer {token}",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                            { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });






        var app = builder.Build();


        // ── Seed Roles & Default Users ────────────────────────────────────────
        using (var scope = app.Services.CreateScope())
        {
            await AuthDbContextSeed.SeedAsync(scope.ServiceProvider);
            await RewardDbSeed.SeedAsync(scope.ServiceProvider);
        }


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
