using Csissors.Schedule;
using System;
using System.Collections.Generic;

namespace Csissors.Tasks
{
    public class TaskConfiguration
    {
        public static readonly TaskConfiguration Default = new TaskConfiguration(
            new NullSchedule(), FailureMode.None, ExecutionMode.AtLeastOnce, TimeSpan.FromSeconds(60), new Dictionary<string, object?>()
        );

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

        public TaskConfiguration WithSchedule(ISchedule schedule) => new TaskConfiguration(schedule, FailureMode, ExecutionMode, LeaseDuration, Data);
        public TaskConfiguration WithData(IReadOnlyDictionary<string, object?> data) => new TaskConfiguration(Schedule, FailureMode, ExecutionMode, LeaseDuration, data);
    }
}