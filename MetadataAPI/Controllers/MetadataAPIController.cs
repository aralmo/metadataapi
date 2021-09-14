using MetadataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

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
        public async Task<EitherActionResult<ResultTypes.JsonString,Error>> Get(string resourceId, string field) =>
            await repository
                .Get(resourceId, field)
                .Map(ok => new ResultTypes.JsonString(ok))
                .MapError<Error>(err => err switch
                {
                    MetadataRepositoryErrors.ResourceNotFound => new ResultTypes.HttpNotFound(err.Message),
                    MetadataRepositoryErrors.ParsingError => new ResultTypes.HttpBadRequest(err.Message),
                    _ => err                    
                }).Result();

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
