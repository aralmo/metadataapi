using DeFuncto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MetadataService
{
    public static class HttpHelper
    {

        public record HttpError(HttpWebResponse Response) : Error($"{(int)Response.StatusCode} - {Response.StatusDescription}");
        public record ConnectionError(string Message) : Error(Message, LogLevel.Critical);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<HttpWebResponse, Error> HttpGet(string url)
        {
            return Go().Async();

            async Task<Result<HttpWebResponse, Error>> Go()
            {
                try
                {
                    var r = await HttpWebRequest
                            .Create(url)
                            .GetResponseAsync() as HttpWebResponse;

                    return r.StatusCode switch
                    {
                        HttpStatusCode.OK => r,
                        _ => new HttpError(r)
                    };
                }
                catch (Exception ex)
                {
                    return ex.MapToResult();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<HttpWebResponse, Error> HttpPost(string url, Action<Stream> bodyWritter = null, string contentType = "application/json") =>
            HttpBodyRequest("POST", url, bodyWritter, contentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncResult<HttpWebResponse, Error> HttpPut(string url, Action<Stream> bodyWritter = null, string contentType = "application/json") =>
            HttpBodyRequest("PUT", url, bodyWritter, contentType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AsyncResult<HttpWebResponse, Error> HttpBodyRequest(string method, string url, Action<Stream> bodyWritter = null, string contentType = "application/json")
        {
            return Go().Async();

            async Task<Result<HttpWebResponse, Error>> Go()
            {
                HttpWebRequest request;
                try
                {
                    request = HttpWebRequest.Create(url) as HttpWebRequest;
                    request.Method = method;
                    request.ContentType = contentType;
                    var stream = await request.GetRequestStreamAsync();
                    bodyWritter?.Invoke(stream);

                    var response = await request.GetResponseAsync() as HttpWebResponse;

                    //todo: how would I trace requests to elastic? maybe do an observer to httphelper? not sure if it's gonna be inlined that way

                    return
                        (int)response.StatusCode / 100 == 2 ?
                            response :
                            new HttpError(response);
                }
                catch (WebException ex)
                {
                    return ex.MapToResult();
                }
                catch (Exception ex)
                {
                    return ex.MapToResult();
                }
            }
        }

        static Error MapToResult(this Exception ex) => ex switch
        {
            //return bad requests as a parsing error type.
            WebException wex when wex.Status == WebExceptionStatus.ProtocolError &&
            (wex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.BadRequest =>
                new MetadataRepositoryErrors.ParsingError(wex.Response?.GetResponseStream()?.ReadToEnd()),

            //return not found as document not found
            WebException wex when wex.Status == WebExceptionStatus.ProtocolError &&
            (wex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.BadRequest =>
                new MetadataRepositoryErrors.ResourceNotFound(wex.Response?.GetResponseStream()?.ReadToEnd()),

            //any other kind of web exception as http error
            WebException wex when wex.Response != null => new HttpError(wex.Response as HttpWebResponse),
            WebException wex => new ConnectionError(wex.Message),

            //everything else
            Exception => new Error(ex.Message)
        };

    }

}


//Hack around vs2019 bug
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

