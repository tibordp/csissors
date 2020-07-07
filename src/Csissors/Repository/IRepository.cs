using Csissors.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Repository
{
    public interface IRepository : IAsyncDisposable
    {
        IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken);
        Task RegisterTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken);
        Task UnregistrerTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken);
        Task CommitTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken);
        Task UnlockTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken);
        Task<PollResponse> PollTaskAsync(DateTimeOffset now, ITask task, ILease? lease, CancellationToken cancellationToken);
    }
}