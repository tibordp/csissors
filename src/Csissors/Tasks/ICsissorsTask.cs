using Csissors.Schedule;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    public interface ICsissorsTask
    {
        string Name { get; }
        ISchedule Schedule { get; }
        FailureMode FailureMode { get; }
        ExecutionMode ExecutionMode { get; }
        bool Dynamic { get; }
        Task ExecuteAsync(ITaskContext context);
    }
}