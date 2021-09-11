using MetadataService;
using MetadataService.Elastic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataAPI
{
    public static class Setup
    {
        public static void SetupDependencies(this IServiceCollection services)
        {
            services.AddTransient<IMetadataRepository, ElasticMetadataStorage>();
            services.AddLogging(cfg => cfg.AddConsole().SetMinimumLevel(LogLevel.Trace));
        }

        public static void SetupMiddlewares(this IApplicationBuilder builder)
        {
            //todo: add trace logs for requests
            builder.AddRequestLogMiddleware(request =>
                RequestLoggingMiddleware.LogOptions.LogBasic |
                RequestLoggingMiddleware.LogOptions.AsInfo |

                //if it's post capture the request body and log it too
                (request.Method == "POST" ? RequestLoggingMiddleware.LogOptions.LogRequestBody : 0));
            
        }
    }
}
