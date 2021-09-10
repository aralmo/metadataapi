using MetadataService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataAPI.Middlewares
{
    public static class LoggingRequestContext
    {
        public static void AddLoggingRequestContextMiddleware(this IApplicationBuilder app)
        {
           //app.UseMiddleware<RequestLoggingMiddleware>();
        }

    }

    public class RequestLoggingMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate next;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger = null)
        {
            this.logger = logger;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (logger.BeginScope("{Method} {Path}", context.Request))
            {
                await next(context);
            }
        }
    }
}
