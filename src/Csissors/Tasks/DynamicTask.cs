using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class DynamicTaskInstance : ITask
    {
        private readonly TaskConfiguration? _configuration;

        public DynamicTaskInstance(IDynamicTask parentTask, string instanceName, TaskConfiguration? configuration = null)
        {
            ParentTask = parentTask ?? throw new ArgumentNullException(nameof(parentTask));
            Name = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
            _configuration = configuration;
        }

        public string Name { get; }
        public TaskConfiguration Configuration => _configuration ?? throw new Exception("No configuration present");
        public IDynamicTask ParentTask { get; }
        public Task ExecuteAsync(ITaskContext context) => ParentTask.ExecuteAsync(context);
    }

}