using MetadataService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MetadataAPI.Controllers
{
    [ApiController]
    [Route("metadata")]
    public class MetadataAPIController : ControllerBase
    {
        readonly IMetadataStorage storage;

        public MetadataAPIController(IMetadataStorage storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        [Route("{resourceId}/{field}")]
        public async Task<IActionResult> Get(string resourceId, string field) =>
            await storage
                .Get(resourceId, field)
                .Map(  metadata => Content(metadata, "application/json"))
                .MapError(error => error switch
                {
                    MetadataStorageErrors
                        .ResourceNotFound => StatusCode((int)HttpStatusCode.NotFound, error.Message),
                                        _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
                })
                .Match(   ok => (IActionResult)ok,
                       error => (IActionResult)error);


        [HttpPost]
        [Route("{resourceId}/{field}")]
        public async Task<IActionResult> Push(string resourceId, string field, [FromBody] JsonElement data) =>
            await storage
                .Push(resourceId, field, data)
                .Map(         _ => Ok())
                .MapError(error => error switch
                {
                    MetadataStorageErrors
                            .ParsingError => StatusCode((int)HttpStatusCode.BadRequest, error.Message),
                                        _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
                })
                .Match(   ok => (IActionResult)ok,
                       error => (IActionResult)error);
    }
}
