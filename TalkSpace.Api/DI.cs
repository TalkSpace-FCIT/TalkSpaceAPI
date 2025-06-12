using Serilog;
using TalkSpace.Api.Extensions;
using Scalar.AspNetCore;

namespace TalkSpace.Api
{
    public static class DI
    {
        public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            var environment = builder.Environment;
            services.AddApplicationServices(configuration, environment);
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });


            return builder;
        }
    }
}