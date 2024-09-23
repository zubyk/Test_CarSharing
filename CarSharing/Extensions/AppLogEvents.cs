using Microsoft.Extensions.Logging;

namespace CarSharing
{
    partial class ILoggerExtension
    {
        private const int _infoEventIdShift = 16000;
        private const int _warningEventIdShift = _infoEventIdShift + 1000;
        private const int _errorEventIdShift = _warningEventIdShift + 1000;

        public static readonly EventId MemberCall = new(_infoEventIdShift + 1, nameof(MemberCall));
        public static readonly EventId EntitiesTimeMatch = new(_infoEventIdShift + 2, nameof(EntitiesTimeMatch));

        public static readonly EventId ExecutionError = new(_errorEventIdShift + 1, nameof(ExecutionError));
    }
}
