using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Executor
{
    public interface IExecutor : IAsyncDisposable
    {
        Task SpawnTaskAsync(Func<Task> action, CancellationToken cancellationToken);
        Task WaitUntilFinished(CancellationToken cancellationToken);
    }
}