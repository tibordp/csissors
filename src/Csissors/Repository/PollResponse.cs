using System;

namespace Csissors.Repository
{
    public class PollResponse
    {
        public ResultType Result { get; set; }
        public DateTimeOffset ScheduledAt { get; set; }
        public ILease? Lease { get; set; }
    }
}