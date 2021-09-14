using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataService
{

    public record Error(string Message, LogLevel Level = LogLevel.Error);

}
