using Csissors.Schedule;
using System;
using System.Collections.Generic;

namespace Csissors.Tasks
{
    public class TaskConfiguration
    {
        public ISchedule Schedule { get; }
        public FailureMode FailureMode { get; }
        public ExecutionMode ExecutionMode { get; }
        public TimeSpan LeaseDuration { get; }
        public IReadOnlyDictionary<string, object?> Data { get; }
        public TaskConfiguration(ISchedule schedule, FailureMode failureMode, ExecutionMode executionMode, TimeSpan leaseDuration, IReadOnlyDictionary<string, object?> data)
        {
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            FailureMode = failureMode;
            ExecutionMode = executionMode;
            LeaseDuration = leaseDuration;
        }
    }
}