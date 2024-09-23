using CarSharing.ViewModels;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CarSharing
{
    partial class ILoggerExtension
    {
        public readonly struct CallerLogEntity
        {
            private readonly string _text;

            public CallerLogEntity(object? caller, [NotNullWhen(true)] string? member)
            {
                if (caller is null)
                {
                    _text = $"[{member}]";
                }
                else if (caller is string callerName)
                {
                    if (!string.IsNullOrWhiteSpace(callerName))
                    {
                        _text = $"[{callerName}.{member}]";
                    }
                    else
                    {
                        _text = $"[{member}]";
                    }
                }
                else
                {
                    _text = $"[{caller.GetType().Name}.{member}]";
                }
            }

            public override string ToString() => _text;

            public override int GetHashCode() => _text.GetHashCode();

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                return obj is CallerLogEntity cle && _text.Equals(cle._text);
            }
        }
        


        private static readonly Action<ILogger, CallerLogEntity, Exception?>
       _logMemberCall = LoggerMessage.Define<CallerLogEntity>(
           LogLevel.Debug,
           MemberCall,
           "Member {caller} call");

        private static readonly Action<ILogger, CallerLogEntity, object[], Exception?>
        _logMemberCallWithParams = LoggerMessage.Define<CallerLogEntity, object[]>(
            LogLevel.Debug,
            MemberCall,
            "Member {caller} call with params ({args})");



        [LoggerMessage(
            EventId = _errorEventIdShift + 1,
            EventName = nameof(ExecutionError),
            Message = "{error}")]
        public static partial void LogExecutionError(this ILogger logger, LogLevel logLevel, string? error, Exception? exception);


        private static readonly Action<ILogger, CarDriverEntity, Exception?>
            _logEntitiesTimeMatch = LoggerMessage.Define<CarDriverEntity>(
            LogLevel.Information,
            EntitiesTimeMatch,
            "Entity {entity} time match");

        public static void LogEntitiesTimeMatch(this ILogger logger, CarDriverEntity entity)
             => _logEntitiesTimeMatch(logger, entity, null);
    }
}
