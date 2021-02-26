using Csissors.Schedule;
using System;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class DynamicTaskInstance : ITask
    {
        public DynamicTaskInstance(IDynamicTask parentTask, string instanceName, TaskConfiguration configuration)
        {
            ParentTask = parentTask ?? throw new ArgumentNullException(nameof(parentTask));
            Name = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string Name { get; }
        public TaskConfiguration Configuration { get; }
        public IDynamicTask ParentTask { get; }
        public Task ExecuteAsync(ITaskContext context) => ParentTask.ExecuteAsync(context);
    }

}