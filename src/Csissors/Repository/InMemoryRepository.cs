using Csissors.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Repository
{
    internal class TaskData
    {
        public DateTimeOffset? ExecuteAfter { get; set; }
        public DateTimeOffset? LockedUntil { get; set; }
        public Lease? LockedBy { get; set; }
    }

    internal class Lease : ILease
    {
    }

    class TaskComparer : IEqualityComparer<ITask>
    {
        public readonly static TaskComparer Instance = new TaskComparer();

        public bool Equals(ITask x, ITask y)
        {
            return string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(ITask obj)
        {
            return obj.Name.GetHashCode();
        }
    }

    public class InMemoryRepository : IRepository, IRepositoryFactory
    {
        private readonly ILogger<InMemoryRepository> _log;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<ITask, TaskData> _data = new Dictionary<ITask, TaskData>();
        private readonly Dictionary<ITask, HashSet<IDynamicTask>> _dynamicTasks = new Dictionary<ITask, HashSet<IDynamicTask>>();

        public InMemoryRepository(ILogger<InMemoryRepository> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IAsyncEnumerable<(IDynamicTask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken)
        {
            (IDynamicTask, ILease)[] result;

            lock (_syncRoot)
            {
                if (_dynamicTasks.TryGetValue(task, out var taskInstances))
                {
                    result = taskInstances.Select(task => (task, (ILease)new Lease())).ToArray();
                }
                else
                {
                    result = Array.Empty<(IDynamicTask, ILease)>();
                }
            }
            return result.ToAsyncEnumerable();
        }

        public Task<PollResponse> PollTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                if (!_data.TryGetValue(task, out var taskData))
                {
                    taskData = new TaskData
                    {
                        ExecuteAfter = task.Configuration.Schedule.GetNextExecution(now, null),
                        LockedBy = null,
                        LockedUntil = null
                    };
                    _data.Add(task, taskData);
                }

                var scheduledAt = taskData.ExecuteAfter.Value;
                ResultType result;

                if (taskData.LockedUntil.HasValue && taskData.LockedUntil.Value > now && (lease != taskData.LockedBy))
                {
                    result = ResultType.Locked;
                }
                else if (taskData.ExecuteAfter.Value <= now && task.Configuration.ExecutionMode == ExecutionMode.AtMostOnce)
                {
                    result = ResultType.Ready;
                    taskData.ExecuteAfter = task.Configuration.Schedule.GetNextExecution(now, taskData.ExecuteAfter);
                    taskData.LockedUntil = null;
                    taskData.LockedBy = null;
                }
                else if (taskData.ExecuteAfter.Value <= now && task.Configuration.ExecutionMode == ExecutionMode.AtLeastOnce)
                {
                    result = ResultType.Ready;
                    taskData.LockedUntil = now + task.Configuration.LeaseDuration;
                    taskData.LockedBy = new Lease();
                }
                else
                {
                    result = ResultType.Pending;
                }
                return Task.FromResult(new PollResponse
                {
                    Result = result,
                    Lease = taskData.LockedBy,
                    ScheduledAt = scheduledAt,
                });
            }
        }

        public Task RegisterTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                if (!_dynamicTasks.TryGetValue(task.ParentTask, out var taskInstances))
                {
                    taskInstances = new HashSet<IDynamicTask>(TaskComparer.Instance);
                    _dynamicTasks.Add(task.ParentTask, taskInstances);
                }
                taskInstances.Add(task);
                return Task.CompletedTask;
            }
        }

        public Task CommitTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                if (_data.TryGetValue(task, out var taskData) && taskData.LockedBy == lease)
                {
                    taskData.LockedBy = null;
                    taskData.LockedUntil = null;
                    taskData.ExecuteAfter = task.Configuration.Schedule.GetNextExecution(now, taskData.ExecuteAfter);
                }

                return Task.CompletedTask;
            }
        }

        public Task UnlockTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                if (_data.TryGetValue(task, out var taskData) && taskData.LockedBy == lease)
                {
                    taskData.LockedBy = null;
                    taskData.LockedUntil = null;
                }

                return Task.CompletedTask;
            }
        }

        public Task UnregistrerTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken)
        {
            lock (_syncRoot)
            {
                if (_dynamicTasks.TryGetValue(task.ParentTask, out var taskInstances))
                {
                    taskInstances.Remove(task);
                    if (!taskInstances.Any())
                    {
                        _dynamicTasks.Remove(task.ParentTask);
                    }
                }
                return Task.CompletedTask;
            }
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public Task<IRepository> CreateRepositoryAsync()
        {
            return Task.FromResult<IRepository>(this);
        }
    }

}