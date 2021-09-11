using MetadataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static MetadataService.HttpHelper;

namespace MetadataAPI.Controllers
{
    [ApiController]
    [Route("metadata")]
    public class MetadataAPIController : ControllerBase
    {
        readonly IMetadataRepository repository;
        private readonly ILogger<MetadataAPIController> logger;

        public MetadataAPIController(IMetadataRepository storage, ILogger<MetadataAPIController> logger)
        {
            this.repository = storage;
            this.logger = logger;
        }

        [HttpGet]
        [Route("{resourceId}/{field}")]
        public async Task<IActionResult> Get(string resourceId, string field) =>
            await repository
                .Get(resourceId, field)
                .Map(metadata => Content(metadata, "application/json"))
                .MapError(error =>
                    HandleError(logger, error) switch
                    {
                        MetadataRepositoryErrors
                            .ResourceNotFound => StatusCode((int)HttpStatusCode.NotFound, error.Message),
                        _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
                    })
                //merge ok and error
                .Match(ok => ok as IActionResult,
                       error => error as IActionResult);


        [HttpPost]
        [Route("{resourceId}/{field}")]
        public async Task<IActionResult> Post(string resourceId, string field, [FromBody] JsonElement data) =>
            await repository
                .Push(resourceId, field, data)
                .Map(_ => Ok())
                .MapError(error =>
                    HandleError(logger, error) switch
                    {
                        MetadataRepositoryErrors
                                .ParsingError => StatusCode((int)HttpStatusCode.BadRequest, error.Message),
                        _ => StatusCode((int)HttpStatusCode.InternalServerError, error.Message)
                    })
                .Match(ok => (IActionResult)ok,
                       error => (IActionResult)error);

        static Error HandleError(ILogger logger, Error e)
        {
            logger?.Log(e.Level, e.Message);
            //todo: hide errors to the response?
            return e;
        }
    }
}
