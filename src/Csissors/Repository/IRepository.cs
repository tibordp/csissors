using Csissors.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Repository
{
    public interface IRepository : IAsyncDisposable
    {
        IAsyncEnumerable<(ICsissorsTask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken);
        Task RegisterTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken);
        Task UnregistrerTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken);
        Task CommitTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken);
        Task UnlockTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken);
        Task<PollResponse> PollTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken);
    }

}