using System;

namespace Csissors.Schedule
{
    public class IntervalSchedule : ISchedule
    {
        public IntervalSchedule(TimeSpan interval, bool fastForward)
        {
            Interval = interval;
            FastForward = fastForward;
        }

        public TimeSpan Interval { get; }
        public bool FastForward { get; }

        public DateTimeOffset? GetNextExecution(DateTimeOffset now, DateTimeOffset? lastExecution = null)
        {
            if (!lastExecution.HasValue)
            {
                return now + Interval;
            }
            var ticks = (now - lastExecution.Value).Ticks;
            var intervals = Math.Max(
                FastForward ?
                (ticks + Interval.Ticks - 1) / Interval.Ticks
                : 1, 1
            );
            return lastExecution.Value + new TimeSpan(intervals * Interval.Ticks);
        }
    }

}