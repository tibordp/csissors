using Cronos;
using System;

namespace Csissors.Schedule
{
    public class CronSchedule : ISchedule
    {
        public CronSchedule(CronExpression cronExpression, TimeZoneInfo timeZoneInfo, bool fastForward)
        {
            CronExpression = cronExpression;
            TimeZoneInfo = timeZoneInfo;
            FastForward = fastForward;
        }

        public CronExpression CronExpression { get; }
        public TimeZoneInfo TimeZoneInfo { get; }
        public bool FastForward { get; }
        public DateTimeOffset? GetNextExecution(DateTimeOffset now, DateTimeOffset? lastExecution = null)
        {
            if (!lastExecution.HasValue)
            {
                return CronExpression.GetNextOccurrence(now, TimeZoneInfo);
            }

            DateTimeOffset? nextExecution = lastExecution;
            do
            {
                nextExecution = CronExpression.GetNextOccurrence(nextExecution.Value, TimeZoneInfo);
            } while (nextExecution.HasValue && !(FastForward || nextExecution >= now));
            return nextExecution;
        }
    }

}