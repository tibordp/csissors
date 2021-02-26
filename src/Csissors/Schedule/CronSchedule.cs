using Cronos;
using System;

namespace Csissors.Schedule
{ 
    /*

        { type: "cronInterval", spec: { cronExpression: "* * * * *", timeZone: "UTC+1", fastForward: true } }
        { type: "intervalExpression", { interval: 2000, fastForward: true } }
        { type: "onceOff", { execeuteAt: "2020-12-12T12:12:12Z" } }

    */
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