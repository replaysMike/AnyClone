using NLog;

namespace AnyClone.Tests.TestObjects
{
    public class LoggerObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    }
}
