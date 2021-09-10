using MetadataService;
using MetadataService.Elastic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetadataAPI.Middlewares;

namespace MetadataAPI
{
    public static class Setup
    {
        public static void SetupDependencies(this IServiceCollection services)
        {
            services.AddTransient<IMetadataStorage, ElasticMetadataStorage>();
            services.AddLogging(cfg => cfg.AddConsole().SetMinimumLevel(LogLevel.Trace));
        }

        public static void SetupMiddlewares(this IApplicationBuilder builder)
        {
            //todo: add trace logs for requests
            builder.AddLoggingRequestContextMiddleware();
        }
    }
}
