using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetadataAPI
{
    public static class LogEvents
    {
        public static readonly EventId BeginRequest = new(100, "Begin Request");
        public static readonly EventId EndRequest = new(101, "End Request");
    }
}
