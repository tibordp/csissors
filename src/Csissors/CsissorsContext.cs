using Csissors.Repository;
using Csissors.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public class CsissorsContext : IAppContext
    {
        private readonly IRepository _repository;
        private readonly IReadOnlyList<ICsissorsTask> _tasks;

        public CsissorsContext(IRepository repository, IReadOnlyList<ICsissorsTask> tasks)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        }
        public async ValueTask DisposeAsync()
        {
            await _repository.DisposeAsync();
        }

        private async Task TickAsync(CancellationToken cancellationToken)
        {
            foreach (var task in _tasks)
            {
                var taskContext = new TaskContext
                {
                    AppContext = this,
                    Cancellation = cancellationToken,
                    Task = task
                };
                await task.ExecuteAsync(taskContext);
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await TickAsync(cancellationToken);
                await Task.Delay(1000);
            }
        }
    }

}