using Domain.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TalkSpace.Api.Extensions;

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
            // Register all infrastructure services
            services.AddApplicationServices(Configuration);

            //Register JWT authentication
            services.Configure<JWT>(Configuration.GetSection("JWT"));

            // Register controllers
            services.AddControllers();

            // OpenAPI/Swagger configuration
            services.AddOpenApi();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app)
        {
            // Seed database
            await app.ApplicationServices.SeedDatabaseAsync();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}