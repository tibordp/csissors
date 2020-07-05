using System;

namespace Csissors.Repository
{
    public class PollResponse
    {
        ResultType Result { get; }
        DateTimeOffset ScheduledAt { get; }
        ILease Lease { get; }
    }
}