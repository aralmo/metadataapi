using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataService
{
    internal static class LogEvents
    {
        public static EventId ElasticHttpRequest = new(100, "HTTP request to Elastic");
    }
}
