using Csissors.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public interface IAppContext : IAsyncDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
        ITask GetTask(string taskName);
        Task ScheduleTask(ITask baseTask, string taskInstanceName, TaskConfiguration taskConfiguration, CancellationToken cancellationToken);
        Task UnscheduleTask(ITask baseTask, string taskInstanceName, CancellationToken cancellationToken);
    }
}