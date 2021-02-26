using Csissors.Tasks;

namespace Csissors.Serialization
{
    public interface ITaskInstanceFactory1
    {
        ITask CreateTaskInstance(IDynamicTask template, string name, TaskConfiguration taskConfiguration);
    }

    public interface ITaskInstanceFactory {
        ITask CreateTaskInstance(IDynamicTask template, string name, TaskConfiguration taskConfiguration);
    }
}