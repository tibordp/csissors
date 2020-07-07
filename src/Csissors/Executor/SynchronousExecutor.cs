using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Executor
{
    public class SynchronousExecutor : IExecutor
    {
        public async Task SpawnTaskAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await action().ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        public Task WaitUntilFinished(CancellationToken cancellationToken) => Task.CompletedTask;
        public ValueTask DisposeAsync() => default;
    }
}