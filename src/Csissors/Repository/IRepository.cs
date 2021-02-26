using Csissors.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Repository
{
    public interface IRepository : IAsyncDisposable
    {
        IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(IDynamicTask task, CancellationToken cancellationToken);
        Task RegisterTaskAsync(ITask task, CancellationToken cancellationToken);
        Task UnregistrerTaskAsync(ITask task, CancellationToken cancellationToken);
        Task CommitTaskAsync(ITask task, ILease lease, CancellationToken cancellationToken);
        Task UnlockTaskAsync(ITask task, ILease lease, CancellationToken cancellationToken);
        Task<PollResponse> PollTaskAsync(ITask task, ILease? lease, CancellationToken cancellationToken);
    }
}