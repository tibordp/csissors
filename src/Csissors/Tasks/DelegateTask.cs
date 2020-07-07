using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public delegate Task TaskFunc(ITaskContext context);

    internal class DelegateTask : ITask, IDynamicTask
    {
        private readonly TaskFunc _delegate;
        private readonly TaskConfiguration? _configuration;

        public DelegateTask(TaskFunc @delegate, string name, TaskConfiguration? configuration)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _configuration = configuration;
        }

        public string Name { get; }
        public TaskConfiguration Configuration => _configuration ?? throw new Exception("This is a dynamic task.");
        public IDynamicTask? ParentTask => null;
        public Task ExecuteAsync(ITaskContext context) => _delegate(context);
    }
}