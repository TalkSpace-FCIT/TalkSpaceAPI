using Application.Abstractions;
using Application.Services;
using Domain.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Context;
using System;
using System.Text;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Scalar.AspNetCore;
using Microsoft.OpenApi.Models;
using TalkSpace.Api.Utilties;
using Persistence.DbInitialization;


namespace TalkSpace.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            ConfigureAppData(services, configuration);
            AddDatabase(services, configuration);
            AddIdentityServices(services);
            ReggisterServices(services);
            AddJwtAuthentication(services, configuration);
            AddMappingServices(services);

            services.AddScoped<IJWtTokenService, JWtTokenService>();
            AddOpenApiDocumentation(services);

            return services;
        }
        public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            await DbSeeder.SeedAsync(scope.ServiceProvider);
        }
        public static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
        {
            //app.UseOpenApi();
            //app.MapScalarApiReference();

            // Redirect root to Scalar UI
            //app.MapGet("/", () => Results.Redirect("/scalar/v1"))
            //   .ExcludeFromDescription();

            return app;
        }

        private static void ReggisterServices(IServiceCollection services)
        {
            services.AddScoped<IJWtTokenService, JWtTokenService>();
        }
        private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetSection("DatabaseConnections:DefaultConnection").Value,
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
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
            services.AddAutoMapper(typeof(Program).Assembly);
        }

        private static void ConfigureAppData(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseConnections>(configuration.GetSection("ConnectionStrings"));
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

        private static void AddOpenApiDocumentation(IServiceCollection services)
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

        }


    }
}