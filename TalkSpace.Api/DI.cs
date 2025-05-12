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

            services.AddApplicationServices(configuration);
            return builder;
        }
    }
}