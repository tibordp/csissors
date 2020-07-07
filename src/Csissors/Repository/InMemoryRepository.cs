using Csissors.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Repository
{
    public class InMemoryRepository : IRepository, IRepositoryFactory
    {
        internal class TaskComparer : IEqualityComparer<ITask>
        {
            public readonly static TaskComparer Instance = new TaskComparer();

            public bool Equals(ITask x, ITask y)
            {
                return x.GetCanonicalName() == y.GetCanonicalName();
            }

            public int GetHashCode(ITask obj)
            {
                return obj.GetCanonicalName().GetHashCode();
            }
        }

        internal class Lease : ILease
        {
        }

        internal class TaskData
        {
            public DateTimeOffset? ExecuteAfter { get; set; }
            public DateTimeOffset? LockedUntil { get; set; }
            public Lease? LockedBy { get; set; }
        }

        private readonly ILogger<InMemoryRepository> _log;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<ITask, TaskData> _data = new Dictionary<ITask, TaskData>();
        private readonly Dictionary<IDynamicTask, HashSet<ITask>> _dynamicTasks = new Dictionary<IDynamicTask, HashSet<ITask>>();

        public InMemoryRepository(ILogger<InMemoryRepository> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken)
        {
            var result = Array.Empty<(ITask, ILease)>();
            lock (_syncRoot)
            {
                if (_dynamicTasks.TryGetValue(task, out var taskInstances))
                {
                    result = taskInstances.Select(task => (task, (ILease)new Lease())).ToArray();
                }
            }
            return result.ToAsyncEnumerable();
        }

        public Task<PollResponse> PollTaskAsync(DateTimeOffset now, ITask task, ILease? lease, CancellationToken cancellationToken)
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

                var scheduledAt = taskData.ExecuteAfter;

                ResultType result;
                if (!taskData.ExecuteAfter.HasValue)
                {
                    result = ResultType.Missing;
                }
                else if (taskData.LockedUntil.HasValue && taskData.LockedUntil.Value > now && (lease != taskData.LockedBy))
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

        public Task RegisterTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken)
        {
            if (task.ParentTask != null)
            {
                lock (_syncRoot)
                {
                    if (!_dynamicTasks.TryGetValue(task.ParentTask, out var taskInstances))
                    {
                        taskInstances = new HashSet<ITask>(TaskComparer.Instance);
                        _dynamicTasks.Add(task.ParentTask, taskInstances);
                    }
                    taskInstances.Add(task);
                }
            }
            return Task.CompletedTask;
        }

        public Task UnregistrerTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken)
        {
            if (task.ParentTask != null)
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
                }
            }
            return Task.CompletedTask;
        }

        public Task<IRepository> CreateRepositoryAsync()
        {
            return Task.FromResult<IRepository>(this);
        }
        public ValueTask DisposeAsync() => default;
    }

}