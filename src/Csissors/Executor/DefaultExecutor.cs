using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Executor
{

    public class DefaultExecutor : IExecutor
    {
        private readonly ILogger<DefaultExecutor> _log;
        private readonly CsissorsOptions _options;
        private readonly SemaphoreSlim _semaphore;

        public DefaultExecutor(ILogger<DefaultExecutor> log, IOptions<CsissorsOptions> options)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _options = options.Value;
            _semaphore = new SemaphoreSlim(_options.MaxExecutionSlots);
        }

        public ValueTask DisposeAsync() => new ValueTask(WaitUntilFinished(CancellationToken.None));
        public async Task WaitUntilFinished(CancellationToken cancellationToken)
        {
            _log.LogInformation("Waiting utill all remaining tasks finish");
            // Try to acquire all slots, meaning that all the tasks have finished 
            // execution.
            await Task.WhenAll(Enumerable
                .Range(0, _options.MaxExecutionSlots)
                .Select(_ => _semaphore.WaitAsync(cancellationToken))
            ).ConfigureAwait(false);
        }

        public async Task SpawnTaskAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            _ = TaskWrapper(action());
        }

        private async Task TaskWrapper(Task wrapper)
        {
            try
            {
                await wrapper.ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}