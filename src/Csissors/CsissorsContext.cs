using Csissors.Repository;
using Csissors.Tasks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public class CsissorsContext : IAppContext
    {
        private readonly IRepository _repository;
        private readonly IReadOnlyList<ITask> _tasks;
        private readonly CsissorsOptions _configuration;

        public CsissorsContext(IRepository repository, IReadOnlyList<ITask> tasks, IOptions<CsissorsOptions> options)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _configuration = options.Value;
        }

        private async Task SpawnTask(ITask task, PollResponse pollResponse, CancellationToken cancellationToken)
        {
            bool executionSucceeded = true;
            try
            {
                var taskContext = new TaskContext
                {
                    AppContext = this,
                    Cancellation = cancellationToken,
                    Task = task
                };
                await task.ExecuteAsync(taskContext);
            }
            catch
            {
                executionSucceeded = false;
            }

            var now = DateTimeOffset.UtcNow;
            if (task.Configuration.ExecutionMode == ExecutionMode.AtMostOnce)
            {
                return;
            }
            else if (executionSucceeded || task.Configuration.FailureMode == FailureMode.Commit)
            {
                await _repository.CommitTaskAsync(now, task, pollResponse.Lease, cancellationToken);
            }
            else if (task.Configuration.FailureMode == FailureMode.Unlock)
            {
                await _repository.UnlockTaskAsync(now, task, pollResponse.Lease, cancellationToken);
            }
        }

        private async IAsyncEnumerable<(ITask, ILease?)> GetActiveTasksAsync(DateTimeOffset now, CancellationToken cancellationToken)
        {
            foreach (var task in _tasks)
            {
                if (!task.Configuration.Dynamic)
                {
                    yield return (task, null);
                }
                else
                {
                    await foreach (var taskAndLease in _repository.PollDynamicTaskAsync(now, task, cancellationToken))
                    {
                        yield return taskAndLease;
                    }
                }
            }
        }

        private async Task TickAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            await foreach (var (task, lease) in GetActiveTasksAsync(now, cancellationToken))
            {
                var pollResponse = await _repository.PollTaskAsync(now, task, lease, cancellationToken);
                switch (pollResponse.Result)
                {
                    case ResultType.Pending:
                        break;
                    case ResultType.Ready:
                        await SpawnTask(task, pollResponse, cancellationToken);
                        break;
                    case ResultType.Locked:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pollResponse.Result));
                }
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await TickAsync(cancellationToken);
                await Task.Delay(_configuration.PollInterval, cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _repository.DisposeAsync();
        }

        public ITask GetTask(string taskName)
        {
            return _tasks.Single(task => task.Name == taskName);
        }

        public async Task ScheduleTask(ITask baseTask, string taskInstanceName, TaskConfiguration taskConfiguration, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var dynamicTaskInstance = new DynamicTask(baseTask, taskInstanceName, taskConfiguration);
            await _repository.RegisterTaskAsync(now, dynamicTaskInstance, cancellationToken);

        }

        public async Task UnscheduleTask(ITask baseTask, string taskInstanceName, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var dynamicTaskInstance = new DynamicTask(baseTask, taskInstanceName);
            await _repository.UnregistrerTaskAsync(now, dynamicTaskInstance, cancellationToken);
        }
    }

}