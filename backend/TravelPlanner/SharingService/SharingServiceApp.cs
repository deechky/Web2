using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharingService.Data;
using SharingService.Health;
using SharingService.Observability;
using SharingService.Services;

namespace SharingService
{
    /// <summary>
    /// DI registracija i middleware pipeline izdvojeni iz <see cref="SharingService"/> (Service Fabric
    /// listener factory) u testabilne statičke metode - isti razlog/obrazac kao TripServiceApp.
    /// Namerno bez <c>IReliableStateManager</c> registracije (StateManager je dostupan samo unutar
    /// stvarne SF stateful service instance) - testovi u ovoj fazi pokrivaju HTTP/health/observability
    /// sloj, ne Reliable Collections keš, pa se ta zavisnost registruje odvojeno na pozivnoj strani.
    /// </summary>
    public static class SharingServiceApp
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.AddTravelPlannerObservability("sharing-service");

            builder.Services.AddControllers();

            builder.Services.AddDbContext<SharingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SharingDB")));

            builder.Services.AddScoped<ShareStore>();
            builder.Services.AddHttpClient<TripClient>();

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("SharingDB")!,
                    name: "sqlserver-sharingdb",
                    tags: new[] { "ready" });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SharingService", Version = "v1" });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Unesi token u formatu: Bearer {token}"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public static void ConfigurePipeline(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("Frontend");
            app.UseAuthentication();
            app.UseAuthorization();

            // /health/live - proces radi, ne proverava zavisnosti (za orkestraciju/restart odluke).
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriter.WriteJson
            });

            // /health/ready - proces + baza spremni da opsluže saobraćaj.
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter.WriteJson
            });

            app.MapControllers();
        }
    }
}
