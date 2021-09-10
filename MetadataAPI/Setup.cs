using MetadataService.Elastic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddScoped<ElasticMetadataStorage>();
            //todo: add logger
        }

        public static void SetupMiddlewares(this IApplicationBuilder builder)
        {
            //todo: add trace logs for requests
           
        }
    }
}
