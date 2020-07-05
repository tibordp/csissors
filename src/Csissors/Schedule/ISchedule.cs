using System;

namespace Csissors.Schedule
{
    public interface ISchedule
    {
        DateTimeOffset? GetNextExecution(DateTimeOffset now, DateTimeOffset? lastExecution = null);
    }

}