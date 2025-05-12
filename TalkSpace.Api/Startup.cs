using Serilog;
using TalkSpace.Api.Extensions;
using Scalar.AspNetCore;

namespace TalkSpace.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register controllers
            services.AddControllers();

            // Register all infrastructure services
            services.AddApplicationServices(Configuration);

            // OpenAPI/Swagger configuration
            services.AddOpenApi();
     
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app)
        {
            await app.ApplicationServices.SeedDatabaseAsync();

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
<<<<<<< HEAD

            app.UseExceptionHandler(_ => { });
=======
            app.UseOpenApiDocumentation();
>>>>>>> 305cc1b76e83b20a1b6f8e50ece2d33152f93510
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // Remove MapOpenApi().CacheOutput() if present
                endpoints.MapScalarApiReference(); // Scalar UI endpoint
            });
        }
    }
}