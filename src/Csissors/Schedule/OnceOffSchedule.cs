using System;

namespace Csissors.Schedule
{
    public class OnceOffSchedule : ISchedule
    {
        public DateTimeOffset ExecuteAt { get; }

        public OnceOffSchedule(DateTimeOffset executeAt)
        {
            ExecuteAt = executeAt;
        }

        public DateTimeOffset? GetNextExecution(DateTimeOffset now, DateTimeOffset? lastExecution = null) => !lastExecution.HasValue ? (DateTimeOffset?)ExecuteAt : null;
    }
}