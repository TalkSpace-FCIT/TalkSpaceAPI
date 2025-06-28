namespace TalkSpace.API;

using Scalar.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Serilog;
using System.Threading.Tasks;
using TalkSpace.Api;
using Stripe;

public class Program
{

    public static async Task Main(string[] args)

    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());
            // Register Services to IoC
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
            builder.AddServices();

            var app = builder.Build();

            // Configure HTTP Request Pipeline 
            app.UseCors("AllowAll");
            app.MapOpenApi().CacheOutput();
            app.MapScalarApiReference();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();


            app.UseAuthentication();
            app.UseAuthorization();
            app.UseExceptionHandler(_ => { });

            app.MapControllers();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}