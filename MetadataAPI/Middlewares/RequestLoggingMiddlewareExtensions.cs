using MetadataService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataAPI
{
    public static class RequestLoggingMiddlewareExtensions
    {
        public static void AddRequestLogMiddleware(this IApplicationBuilder app, Func<HttpContext, RequestLoggingMiddleware.LogOptions> options = null)
        {
            app.UseMiddleware<RequestLoggingMiddleware>(
                //options defaults to logbasic data as trace.
                options ?? (_ => RequestLoggingMiddleware.LogOptions.LogBasic | RequestLoggingMiddleware.LogOptions.AsTrace));
        }
    }

    public class RequestLoggingMiddlewareConfiguration
    {
        internal Func<HttpContext, RequestLoggingMiddleware.LogOptions> LogOptions;
        public void Configure(Func<HttpContext, RequestLoggingMiddleware.LogOptions> f) => LogOptions = f;
    }

    public class RequestLoggingMiddleware
    {
        private readonly ILogger logger;
        private readonly RequestDelegate next;
        private readonly Func<HttpContext, LogOptions> options;
        private readonly RecyclableMemoryStreamManager streamManager;

        public RequestLoggingMiddleware(RequestDelegate next, Func<HttpContext, LogOptions> options, ILogger<RequestLoggingMiddleware> logger = null)
        {
            this.logger = logger;
            this.next = next;
            this.options = options;
            streamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {

            var toLog = options(context);

            //Read request body if set to log
            string requestBody = string.Empty;
            Stream requestBodyStream = null;
            if (IsSet(toLog, LogOptions.LogResponseBody) && context.Request?.Body != null)
            {
                //clone and read the request body
                requestBodyStream = streamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestBodyStream);

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                requestBody = requestBodyStream.ReadToEnd();
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                context.Request.Body = requestBodyStream;
            }

            //Capture response body if set to log
            Stream originalResponseStream = context.Response.Body;
            if (IsSet(toLog, LogOptions.LogResponseBody) && context.Response?.Body != null)
            {
                //capture the response stream
                var responseBody = streamManager.GetStream();
                context.Response.Body = responseBody;
            }

            //run next in the pipeline
            await next(context);

            //log selected options if any
            if (toLog != 0)
            {
                StringBuilder builder = new();
                List<object> arguments = new();


                if (IsSet(toLog, LogOptions.LogRequest) && context.Request != null)
                {
                    builder.Append("{Method} {Schema} {Host} {Path} {QueryString} ");
                    arguments.AddRange(new object[] {
                        context.Request.Method,
                        context.Request.Scheme,
                        context.Request.Host,
                        context.Request.Path,
                        context.Request.QueryString
                    });
                }

                if (IsSet(toLog, LogOptions.LogResponse) && context.Response != null)
                {
                    builder.Append("{StatusCode} {StatusText} ");
                    arguments.AddRange(new object[] {
                        (int) context.Response.StatusCode,
                        context.Response.StatusCode.ToString()
                    });
                }

                if (IsSet(toLog, LogOptions.LogRequestBody) && context.Request?.Body != null)
                {
                    builder.Append("{RequestBody} ");
                    arguments.AddRange(new object[] {
                        requestBody
                    });
                    await requestBodyStream.DisposeAsync();
                }


                if (IsSet(toLog, LogOptions.LogResponseBody) && context.Response?.Body != null)
                {
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    string body = await context.Response.Body.ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    //copy the captured stream to the actual response stream 
                    await context.Response.Body.CopyToAsync(originalResponseStream);
                    await context.Response.Body.DisposeAsync();

                    builder.Append("{ResponseBody} ");
                    arguments.Add(body);
                }

                //log
                if (IsSet(toLog, LogOptions.AsTrace))
                    logger?.LogTrace(builder.ToString(), arguments.ToArray());

                if (IsSet(toLog, LogOptions.AsDebug))
                    logger?.LogDebug(builder.ToString(), arguments.ToArray());

                if (IsSet(toLog, LogOptions.AsError))
                    logger?.LogError(builder.ToString(), arguments.ToArray());

                if (IsSet(toLog, LogOptions.AsInfo))
                    logger?.LogInformation(builder.ToString(), arguments.ToArray());
            }
        }

        /// hasflag does boxing/unboxing trying to avoid it here, since it's used only inside this class better to use a typesafe method
        private static bool IsSet(LogOptions source, LogOptions flag) =>
            (source & flag) == flag;

        [Flags]
        public enum LogOptions
        {
            //todo: add log for endpoint data, headers, userdata etc...

            AsTrace = 128<<1,
            AsDebug = 128<<2,
            AsInfo = 128<<3,
            AsError = 128<<4,
            AsCritical = 128<<5,

            LogAll = LogBasic | LogAllBodies,

            LogBasic = LogRequest | LogResponse,
            LogRequest = 1<<1,
            LogResponse = 1<<2,

            LogAllBodies = LogRequestBody | LogResponseBody,
            LogRequestBody = 1<<3,
            LogResponseBody = 1<<4,

        }
    }
}
