using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeFuncto;

namespace MetadataService
{
    public interface IMetadataRepository
    {
        /// <summary>
        /// Add or modify a resource metadata field. This will also create the resource if it doesn't exists.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="field"></param>
        /// <param name="data"></param>
        /// <seealso cref="MetadataRepositoryErrors"/>
        /// <returns></returns>
        AsyncResult<Success, Error> Push(string resourceId, string field, JsonElement data);

        /// <summary>
        /// Gets a resource field
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="field"></param>
        /// <seealso cref="MetadataRepositoryErrors"/>
        /// <returns>Json string</returns>
        AsyncResult<string, Error> Get(string resourceId, string field);
    }
}


//Hack around vs2019 bug
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
