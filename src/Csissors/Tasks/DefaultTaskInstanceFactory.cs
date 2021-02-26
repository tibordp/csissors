using Csissors.Serialization;

namespace Csissors.Tasks
{
    public class DefaultTaskInstanceFactory : ITaskInstanceFactory
    {
        public ITask CreateTaskInstance(IDynamicTask template, string name, TaskConfiguration taskConfiguration)
        {
            return new DynamicTaskInstance(template, name, taskConfiguration);
        }
    }
}