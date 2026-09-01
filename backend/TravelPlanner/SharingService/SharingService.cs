using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using SharingService.Data;
using SharingService.Health;
using SharingService.Services;

namespace SharingService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class SharingService : StatefulService
    {
        public SharingService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[]
            {
                new ServiceReplicaListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services
                                    .AddSingleton<StatefulServiceContext>(serviceContext)
                                    .AddSingleton<IReliableStateManager>(this.StateManager);

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

                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        var app = builder.Build();

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

                        return app;

                    }))
            };
        }
    }
}
