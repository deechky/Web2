using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TripService.Data;
using TripService.Health;
using TripService.Observability;
using TripService.Services;

namespace TripService
{
    /// <summary>
    /// DI registracija i middleware pipeline izdvojeni iz <see cref="TripService"/> (Service Fabric
    /// listener factory) u testabilne statičke metode. Service Fabric i dalje kreira <see
    /// cref="WebApplicationBuilder"/>/konfiguriše <c>WebHost</c> (treba mu <c>listener</c>/<c>url</c>
    /// iz SF konteksta), ali sama registracija servisa i middleware pipeline su identični bez obzira
    /// da li nas hostuje SF ili test. Testovi (TripService.Tests) pozivaju iste dve metode direktno,
    /// bez Service Fabric-a - nema duplirane/divergentne konfiguracije između "kako radi u produkciji"
    /// i "kako se testira".
    /// </summary>
    public static class TripServiceApp
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.AddTravelPlannerObservability("trip-service");

            builder.Services.AddControllers();

            builder.Services.AddDbContext<TripDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TripsDB")));

            builder.Services.AddScoped<PlanAccess>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<ShareClient>();

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("TripsDB")!,
                    name: "sqlserver-tripsdb",
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
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "TripService", Version = "v1" });
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
