using Microsoft.Extensions.Logging;

namespace MetadataService
{
    public class MetadataRepositoryErrors
    {
        public record ResourceNotFound(string Message) : Error(Message, LogLevel.Trace);
        public record ParsingError(string Message) : Error(Message, LogLevel.Trace);
    }
}
