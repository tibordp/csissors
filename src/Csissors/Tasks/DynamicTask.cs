using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class DynamicTask : IDynamicTask
    {
        private readonly string _instanceName;
        private readonly TaskConfiguration? _configuration;

        public DynamicTask(ITask parentTask, string instanceName, TaskConfiguration? configuration = null)
        {
            ParentTask = parentTask ?? throw new ArgumentNullException(nameof(parentTask));
            _instanceName = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
            _configuration = configuration;
        }

        public string Name => $"{ParentTask.Name}:{_instanceName}";
        public TaskConfiguration Configuration => _configuration ?? throw new Exception("No configuration present");
        public ITask ParentTask { get; }
        public Task ExecuteAsync(ITaskContext context) => ParentTask.ExecuteAsync(context);
    }

}