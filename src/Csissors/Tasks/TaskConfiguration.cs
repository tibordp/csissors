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
        public bool Dynamic { get; }
        public IReadOnlyDictionary<string, object?> Data { get; }
        public TaskConfiguration(ISchedule schedule, FailureMode failureMode, ExecutionMode executionMode, bool dynamic, TimeSpan? leaseDuration = null, IReadOnlyDictionary<string, object?> data = null)
        {
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            FailureMode = failureMode;
            ExecutionMode = executionMode;
            Dynamic = dynamic;
            LeaseDuration = leaseDuration ?? TimeSpan.FromSeconds(60);
        }
    }
}