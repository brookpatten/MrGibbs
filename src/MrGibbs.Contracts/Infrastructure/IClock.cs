using System;

namespace MrGibbs.Contracts.Infrastructure
{
    /// <summary>
    /// wraps the clock so it can be easily mocked
    /// </summary>
    public interface IClock
    {
        DateTime GetUtcTime();
        DateTime GetLocalTime(TimeZoneInfo timeZone);
        void SetUtcTime(DateTime time);
        DateTime? LastSetAt { get; }
    }
}
