using Csissors.Executor;
using Csissors.Repository;
using Csissors.Serialization;
using Csissors.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public class CsissorsContext : IAppContext
    {
        private readonly ILogger<CsissorsContext> _log;
        private readonly IExecutor _executor;
        private readonly IRepository _repository;
        private readonly ITaskInstanceFactory _taskInstanceFactory;
        private readonly CsissorsOptions _configuration;

        public TaskSet Tasks { get; }

        public CsissorsContext(ILogger<CsissorsContext> log, IExecutor executor, IRepository repository, ITaskInstanceFactory taskInstanceFactory, TaskSet taskSet, IOptions<CsissorsOptions> options)
        {
            Tasks = taskSet ?? throw new ArgumentNullException(nameof(taskSet));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _taskInstanceFactory = taskInstanceFactory ?? throw new ArgumentNullException(nameof(taskInstanceFactory));
            _configuration = options.Value;
        }

        private async Task ExecuteTaskAsync(ITask task, PollResponse pollResponse, CancellationToken cancellationToken)
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
            catch (Exception e)
            {
                _log.LogWarning(e, "Task execution failed");
                executionSucceeded = false;
            }

            try
            {
                if (task.Configuration.ExecutionMode == ExecutionMode.AtMostOnce)
                {
                    return;
                }
                else if (executionSucceeded || task.Configuration.FailureMode == FailureMode.Commit)
                {
                    await _repository.CommitTaskAsync(task, pollResponse.Lease, CancellationToken.None);
                }
                else if (task.Configuration.FailureMode == FailureMode.Unlock)
                {
                    await _repository.UnlockTaskAsync(task, pollResponse.Lease, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _log.LogWarning(e, "Commiting task failed");
            }
        }

        private async IAsyncEnumerable<(ITask, ILease?)> GetActiveTasksAsync(CancellationToken cancellationToken)
        {
            foreach (var task in Tasks.StaticTasks)
            {
                yield return (task, null);
            }

            foreach (var task in Tasks.DynamicTasks)
            {
                await foreach (var taskAndLease in _repository.PollDynamicTaskAsync(task, cancellationToken).ConfigureAwait(false))
                {
                    yield return taskAndLease;
                }
            }
        }

        private async Task TickAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Poll!");
            await foreach (var (task, lease) in GetActiveTasksAsync(cancellationToken).ConfigureAwait(false))
            {
                var pollResponse = await _repository.PollTaskAsync(task, lease, cancellationToken).ConfigureAwait(false);
                switch (pollResponse.Result)
                {
                    case ResultType.Ready:
                        await _executor.SpawnTaskAsync(() => ExecuteTaskAsync(task, pollResponse, cancellationToken), cancellationToken).ConfigureAwait(false);
                        break;
                    case ResultType.Pending:
                        break;                        
                    case ResultType.Locked:
                        break;
                    case ResultType.Missing:
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
                try {
                    await TickAsync(cancellationToken).ConfigureAwait(false);
                } catch (OperationCanceledException) {
                    cancellationToken.ThrowIfCancellationRequested();
                } catch (Exception e) {
                    _log.LogWarning(e, "Polling failed");
                }

                await Task.Delay(_configuration.PollInterval, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task ScheduleTask(IDynamicTask baseTask, string taskInstanceName, TaskConfiguration taskConfiguration, CancellationToken cancellationToken)
        {
            var dynamicTaskInstance = _taskInstanceFactory.CreateTaskInstance(baseTask, taskInstanceName, taskConfiguration);
            await _repository.RegisterTaskAsync(dynamicTaskInstance, cancellationToken).ConfigureAwait(false);
        }

        public async Task UnscheduleTask(IDynamicTask baseTask, string taskInstanceName, CancellationToken cancellationToken)
        {
            var dynamicTaskInstance = _taskInstanceFactory.CreateTaskInstance(baseTask, taskInstanceName, TaskConfiguration.Default);
            await _repository.UnregistrerTaskAsync(dynamicTaskInstance, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await using (_executor.ConfigureAwait(false))
            await using (_repository.ConfigureAwait(false))
                ;
        }
    }

}