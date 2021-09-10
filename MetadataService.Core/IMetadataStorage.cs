using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DeFuncto;

namespace MetadataService
{
    public interface IMetadataStorage
    {
        /// <summary>
        /// Add or modify a resource metadata field. This will also create the resource if it doesn't exists.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="field"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        AsyncResult<Success, Error> Push(string resourceId, string field, JsonElement data);
        AsyncResult<string, Error> Get(string resourceId, string field);
    }


    public class MetadataStorageErrors
    {
        public record ResourceNotFound(string Message) : Error(Message);
        public record ParsingError(string Message) : Error(Message);
    }
}


//Hack around vs2019 bug
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
