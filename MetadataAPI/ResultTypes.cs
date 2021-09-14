using MetadataService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataAPI
{
    public class ResultTypes
    {
        public record HttpNotFound(string Message):Error(Message);
        public record HttpBadRequest(string Message):Error(Message);
        public record HttpInternalError(string Message):Error(Message);

        public record JsonString(string JSON);

    }


}

//todo: Hack around vs2019 bug
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

