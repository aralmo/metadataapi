using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using DeFuncto;
using Microsoft.Extensions.Logging;
using static DeFuncto.Prelude;
using static MetadataService.HttpHelper;

namespace MetadataService.Elastic
{
    public class ElasticMetadataStorage : IMetadataRepository
    {
        //todo: dependency injection
        readonly string IndexName;
        readonly string host;
        readonly Encoding Encoding;

        public ElasticMetadataStorage(string host = "localhost:9200", string indexName = "metadata", Encoding encoding = null)
        {
            this.host = host;
            IndexName = indexName;
            Encoding = encoding ?? Encoding.Default;
        }

        #region IMetadataStorage members

        public AsyncResult<Success, Error> Push(string resourceId, string field, JsonElement data) =>
            UpdateResource(resourceId, field, data)
            //if there is a method not found on the resource, fallback to post    
            .BindError(error =>
                    error switch
                    {
                        HttpError httpError when httpError.Response.StatusCode == HttpStatusCode.NotFound =>
                            CreateResource(resourceId, field, data)
                                //if conflict means two services posted at the same time and the resource exist, fallback again to update
                                .BindError(error => error switch
                                {
                                    HttpError httpError when httpError.Response.StatusCode == HttpStatusCode.Conflict =>
                                        UpdateResource(resourceId, field, data),
                                    _ => error
                                }),
                        _ => error
                    })
            //after fallbacks map and handle the remaining errors as required
            .MapError(error => MapHttpErrors(error))

            //if no errors just return success
            .Map(_ => new Success());

        public AsyncResult<string, Error> Get(string resourceId, string field)
        {
            return Go().Async();
            async Task<Result<string, Error>> Go()
            {
                //todo: filter to get only the selected field
                return
                    await HttpGet($"http://{host}/{IndexName}/_doc/{resourceId}")
                    .Match<Result<string, Error>>(
                        response => ParseSourceFromGetResponse(response.GetResponseStream().ReadToEnd()),
                        //todo: this could be moved to a maperror and use a common delegate with the push method
                        error => MapHttpErrors(error));
            }
        }

        #endregion

        //elastic _update api
        private AsyncResult<HttpWebResponse, Error> UpdateResource(string resourceId, string field, JsonElement data) =>
            from body in BuildUpdateBody(field, data).Async()
            from url in GetResourceUrl(resourceId, "_update").Async()
            from response in HttpPost(url, stream => stream.Write(body, Encoding))
            select response;

        //elastic _create post, no idempotent post will fail if document already exists
        private AsyncResult<HttpWebResponse, Error> CreateResource(string resourceId, string field, JsonElement data) =>
            from body in BuildCreateBody(field, data).Async()
            from url in GetResourceUrl(resourceId, "_create").Async()
            from response in HttpPost(url, stream => stream.Write(body, Encoding))
            select response;

        static private Result<string, Error> ParseSourceFromGetResponse(string json)
        {
            try
            {
                return
                    JsonDocument
                        .Parse(json)
                        .RootElement
                        .GetProperty("_source")
                        .ToString();

            }
            catch (JsonException ex)
            {
                return new MetadataRepositoryErrors.ParsingError($"Error parsing response: {ex.Message}");
            }
        }

        static Result<string, Error> BuildCreateBody(string field, JsonElement data)
        {
            //todo: test all valuekinds and see how elastic behaves on formatting
            string value = data switch
            {
                JsonElement e when e.ValueKind == JsonValueKind.String => $"\"{data}\"",
                _ => data.ToString()
            };

            return $"{{\"{field}\":{value}}}";
        }

        static Result<string, Error> BuildUpdateBody(string field, JsonElement data)
        {
            string value = data switch
            {
                JsonElement e when e.ValueKind == JsonValueKind.String => $"\"{data}\"",
                _ => data.ToString()
            };

            return $"{{\"doc\":{{\"{field}\":{value} }} }}";
        }

        Result<string, Error> GetResourceUrl(string resourceId, string method) =>
            //todo: left in case I need to add some more validation logice, remove otherwise
            string.IsNullOrEmpty(resourceId) || string.IsNullOrEmpty(method) ?
            new Error("invalid resource or method") :
            $"http://{host}/{IndexName}/{method}/{HttpUtility.UrlEncode(resourceId)}";

        static private Error MapHttpErrors(Error error) =>
            error switch
            {
                HttpError http when http.Response.StatusCode == HttpStatusCode.NotFound => new MetadataRepositoryErrors.ResourceNotFound("not found"),
                HttpError http when http.Response.StatusCode == HttpStatusCode.BadRequest => new MetadataRepositoryErrors.ParsingError(error.Message),
                _ => error
            };
    }
}

