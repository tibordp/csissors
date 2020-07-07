using Csissors.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public interface IAppContext : IAsyncDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
        TaskSet Tasks { get; }
        Task ScheduleTask(IDynamicTask baseTask, string taskInstanceName, TaskConfiguration taskConfiguration, CancellationToken cancellationToken);
        Task UnscheduleTask(IDynamicTask baseTask, string taskInstanceName, CancellationToken cancellationToken);
    }
}