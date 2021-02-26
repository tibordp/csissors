using System.Collections.Generic;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public interface ITask
    {
        string Name { get; }
        TaskConfiguration Configuration { get; }
        IDynamicTask? ParentTask { get; }
        Task ExecuteAsync(ITaskContext context);
    }
}