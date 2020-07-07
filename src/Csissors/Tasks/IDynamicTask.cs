using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public interface IDynamicTask
    {
        string Name { get; }
        Task ExecuteAsync(ITaskContext context);
    }
}