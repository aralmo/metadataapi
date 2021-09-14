using DeFuncto;
using MetadataService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using static MetadataAPI.ResultTypes;
using static MetadataService.MetadataRepositoryErrors;

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
        [Produces("application/json")]
        [Route("{resourceId}/{field}")]
        public async Task<EitherActionResult<JsonString,Error>> Get(string resourceId, string field) =>
            await repository
                .Get(resourceId, field)
                .Map(ok => new JsonString(ok))
                .MapError<Error>(err => 
                    err switch
                    {
                        ResourceNotFound => new HttpNotFound(err.Message),
                        _ => HandleExceptionErrors(logger, err)
                    })
                .Result();


        [HttpPost]
        [Produces("text/plain")]
        [Route("{resourceId}/{field}")]
        public async Task<EitherActionResult<Unit, Error>> Post(string resourceId, string field, [FromBody] JsonElement data) =>
            await repository
                .Push(resourceId, field, data)
                .MapError(error =>
                    error switch
                    {
                        ParsingError => new HttpInternalError("Error trying to parse response"),
                        _ => HandleExceptionErrors(logger, error)
                    })
                .Result();

        static Error HandleExceptionErrors(ILogger logger, Error e)
        {
            logger?.Log(e.Level, e.Message);
            return e;
        }
    }
}
