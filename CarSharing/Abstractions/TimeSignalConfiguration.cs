using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CarSharing.Tests")]

namespace CarSharing.Abstractions
{
    internal class TimeSignalConfiguration
    {
        public int TimerResolutionSeconds { get; set; }
    }
}