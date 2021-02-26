using System;
using System.Threading.Tasks;

namespace Csissors.Utilities
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
        Instant CurrentInstant { get; }
        Task DelayAsync(TimeSpan timeSpan);
    }

    internal class Clock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
        public Instant CurrentInstant => Instant.Now;
        public Task DelayAsync(TimeSpan timeSpan) => Task.Delay(timeSpan);
    }
}