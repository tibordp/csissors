using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public delegate Task TaskFunc(ITaskContext context);
    internal class DelegateTask : ICsissorsTask
    {
        private readonly TaskFunc _delegate;

        public DelegateTask(TaskFunc @delegate, ISchedule schedule, string name, FailureMode failureMode, ExecutionMode executionMode, bool dynamic)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FailureMode = failureMode;
            ExecutionMode = executionMode;
            Dynamic = dynamic;
        }

        public string Name { get; }
        public ISchedule Schedule { get; }

        public FailureMode FailureMode { get; }

        public ExecutionMode ExecutionMode { get; }

        public bool Dynamic { get; }

        public Task ExecuteAsync(ITaskContext context) => _delegate(context);
    }

}