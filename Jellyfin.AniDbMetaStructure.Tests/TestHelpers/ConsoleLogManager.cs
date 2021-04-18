using Microsoft.Extensions.Logging;
using NSubstitute;
using System;

namespace Jellyfin.AniDbMetaStructure.Tests.TestHelpers
{
    internal class ConsoleLogger : ILogger
    {
        private readonly ILogger logger;

        public ConsoleLogger()
        {
            this.logger = Substitute.For<ILogger>();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    Console.WriteLine($"Debug: {state}");
                    break;
                case LogLevel.Information:
                    Console.WriteLine($"Info: {state}");
                    break;
                case LogLevel.Error:
                    Console.WriteLine($"Error: {state}");
                    break;
                default:
                    break;
            }
        }
    }
}