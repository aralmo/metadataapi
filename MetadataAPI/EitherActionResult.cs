using DeFuncto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MetadataAPI.ResultTypes;

namespace MetadataAPI
{
    public readonly struct EitherActionResult<TOk, TError> : IActionResult
    {
        private readonly Result<TOk, TError> result;

        public EitherActionResult(Result<TOk, TError> r)
        {
            this.result = r;
        }

        public async Task ExecuteResultAsync(ActionContext context) =>
            await result
                .Match<IActionResult>(
                ok => ok switch 
                {
                    Unit => new EmptyResult(),
                    //already formatted json string
                    JsonString json => new ContentResult()
                    {
                        Content = json.JSON,
                        ContentType = "application/json"
                    },
                    //serialize the output object into json
                    _ => new JsonResult(ok)
                },
                err => err switch
                {
                    HttpBadRequest => new BadRequestResult(),
                    HttpNotFound => new NotFoundResult(),
                    _ => new StatusCodeResult(500)
                }).ExecuteResultAsync(context);

        public static implicit operator EitherActionResult<TOk, TError>(Result<TOk, TError> r) => new(r);
        public static implicit operator Result<TOk, TError>(EitherActionResult<TOk, TError> r) => r.result;
    }
}
