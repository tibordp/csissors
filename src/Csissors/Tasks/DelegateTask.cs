using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public delegate Task TaskFunc(ITaskContext context);
    internal class DelegateTask : ITask
    {
        private readonly TaskFunc _delegate;

        public DelegateTask(TaskFunc @delegate, string name, TaskConfiguration configuration)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string Name { get; }
        public TaskConfiguration Configuration { get; }
        public Task ExecuteAsync(ITaskContext context) => _delegate(context);
    }

}