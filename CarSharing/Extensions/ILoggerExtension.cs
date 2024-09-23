using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CarSharing
{
    internal static partial class ILoggerExtension
    {
        /// <inheritdoc cref="FilterAndLogError(ILogger, Exception, bool)"/>
        public static bool FilterAndLogError(this ILogger logger, LogLevel logLevel, Exception e, bool filterResult = true)
        {
            logger.LogExecutionError(logLevel, e.Message, e);
            return filterResult;
        }

        /// <summary>
        /// Фильтр ошибок для использования в блоке <see langword="try"/> <see langword="catch"/>, выводит ошибку в лог.
        /// <code>
        /// try {}
        /// catch (Exception e) when (logger.FilterAndLogError(e)) {}
        /// </code>
        /// </summary>
        /// <param name="filterResult">Результат, который возвращает метод.</param>
        public static bool FilterAndLogError(this ILogger logger, Exception e, bool filterResult = true)
            => FilterAndLogError(logger, LogLevel.Error, e, filterResult);

        /// <summary>
        /// Выводит в лог запись о вызове метода(свойства).
        /// <code>Logger.LogMethodCall(caller:this, args:new{apiLock, apiName, checkApiCallTimeout});</code>
        /// </summary>
        /// <param name="caller">Владелец объекта, вызвавшего метод.</param>
        /// <param name="member">Имя вызываемого параметра. Оставить пустым для вывода имени параметра из которого выполнен вызов метода.</param>
        /// <param name="args">Параметры вызова метода.</param>
        public static void LogMethodCall(this ILogger logger, object? caller = null, [CallerMemberName] string? member = null, params object[] args)
        {
            if (args?.Length > 0)
            {
                _logMemberCallWithParams(logger, new CallerLogEntity(caller, member), args, null);
            }
            else
            {
                _logMemberCall(logger, new CallerLogEntity(caller, member), null);
            }
        }
    }
}
