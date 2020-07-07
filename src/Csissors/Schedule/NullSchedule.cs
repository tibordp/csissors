using Cronos;
using System;

namespace Csissors.Schedule
{
    public class NullSchedule : ISchedule
    {
        public DateTimeOffset? GetNextExecution(DateTimeOffset now, DateTimeOffset? lastExecution = null) => null;
    }
}