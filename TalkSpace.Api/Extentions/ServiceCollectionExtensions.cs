using Application.Abstractions;
using Application.Services;
using Domain.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Context;
using System.Text;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Scalar.AspNetCore;
using Microsoft.OpenApi.Models;
using TalkSpace.Api.Utilties;
using Persistence.DbInitialization;
using Microsoft.AspNetCore.Mvc;
using TalkSpace.Api.Middleware;
using Domain.Interfaces;
using Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TalkSpace.API;
using Application.Mappings;


namespace TalkSpace.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            AddOpenApiDocumentation(services);
            ConfigureAppData(services, configuration);
            AddDatabase(services, configuration);
            AddIdentityServices(services);
            AddJwtAuthentication(services, configuration);
            AddExceptionHandling(services);
            AddMappingServices(services);
            ReggisterServices(services);
            return services;
        }
        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            await DbSeeder.SeedAsync(scope.ServiceProvider);
        }

        public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddDocumentTransformer((document, context, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "TalkSpace API",
                        Version = "v1",
                        Description = """
                    Comprehensive API for TalkSpace platform.
                    Supports JSON responses.
                    JWT authentication required for protected endpoints.
                    """,
                        Contact = new OpenApiContact
                        {
                            Name = "API Support",
                            Email = "support@talkspace.com"
                        }
                    };
                    return Task.CompletedTask;
                });

            });
            return services;
        }
        private static void ReggisterServices(IServiceCollection services)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMedicalRecordsService, MedicalRecordService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            

        }
        private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetSection("DatabaseConnections:DefaultConnection").Value,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure();
                    })
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        }

        private static void AddIdentityServices(IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }

        private static void AddMappingServices(IServiceCollection services)
        {
            services.AddAutoMapper(configration =>
            {
                configration.AddProfile<MedicalRecordMappingProfile>();
                configration.AddProfile<AppointmentMappingProfile>();
            });
        }

        private static void ConfigureAppData(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseConnections>(configuration.GetSection("ConnectionStrings"));
            services.Configure<JWT>(configuration.GetSection("JWT"));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value!.Errors.Count > 0)
                        .Select(x => new {
                            Field = x.Key,
                            Messages = x.Value!.Errors.Select(e => e.ErrorMessage)
                        });

                    return new BadRequestObjectResult(new { Errors = errors });
                };
            });

        }

        private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWT:Key"]))
                };
            });
        }
        public static IHostBuilder AddSerilog(
             this IHostBuilder builder,
             IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            builder.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.WriteTo.Console();
                loggerConfiguration.WriteTo.MSSqlServer(connectionString,
                    new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true,
                        AutoCreateSqlDatabase = true,
                        SchemaName = "dbo",

                    });
            });

            return builder;
        }

        private static IServiceCollection AddExceptionHandling(IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();
            return services;
        }

    }
}