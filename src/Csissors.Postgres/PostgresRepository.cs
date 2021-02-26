using Csissors.Postgres.Transactions;
using Csissors.Repository;
using Csissors.Serialization;
using Csissors.Tasks;
using Csissors.Utilities;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres
{
    public class PostgresRepository : IRepository
    {
        private class Lease : ILease
        {
            public string LeaseId { get; }

            public Lease(string leaseId)
            {
                if (string.IsNullOrEmpty(leaseId))
                {
                    throw new ArgumentException($"'{nameof(leaseId)}' cannot be null or empty", nameof(leaseId));
                }

                LeaseId = leaseId;
            }

            public override bool Equals(object? obj) => obj is Lease lease &&
                    LeaseId == lease.LeaseId;
            public override int GetHashCode() => LeaseId.GetHashCode();
            public static Lease CreateNew() => new Lease(Guid.NewGuid().ToString());
            public static Lease? FromLeaseId(string? leaseId)
            {
                return string.IsNullOrEmpty(leaseId) ? null : new Lease(leaseId);
            }
        }

        private const string CREATE_TABLE = @"CREATE TABLE IF NOT EXISTS @table_name (
                name text PRIMARY KEY,
                parent_name text,
                locked_until timestamptz,
                locked_by text,
                execute_after timestamptz,
                task_spec json
            )";

        private const string CREATE_INDEX = @"CREATE INDEX IF NOT EXISTS @table_name_parent
            ON @table_name  (parent_name, GREATEST(locked_until, execute_after))
        ";

        private const string POLL_DYNAMIC_TASK = @"UPDATE @table_name a
            SET
                locked_until = @locked_until,
                locked_by = @locked_by
            FROM (
                SELECT name FROM @table_name
                WHERE parent_name = @parent_name AND GREATEST(locked_until, execute_after) <= @now
                ORDER BY GREATEST(locked_until, execute_after) ASC
                LIMIT @limit
                FOR UPDATE SKIP LOCKED
            ) b
            WHERE a.name = b.name
            RETURNING a.task_spec
        ";

        private const string REGISTER_TASK = @"
            INSERT INTO @table_name (name, parent_name, task_spec, execute_after)
            VALUES (@name, @parent_name, @task_spec, @execute_after)
            ON CONFLICT (name) DO UPDATE
            SET
                task_spec = @task_spec,
                execute_after = @execute_after,
                locked_by = NULL,
                locked_until = NULL
        ";

        private const string FETCH_TASK = @"SELECT 
                execute_after,
                locked_until,
                locked_by
            FROM @table_name 
            WHERE name = @name FOR UPDATE"
        ;

        private const string UPDATE_TASK = @"INSERT INTO @table_name 
                (name, locked_until, locked_by, execute_after)
                VALUES (@name, @locked_until, @locked_by, @execute_after)
                ON CONFLICT (name) DO UPDATE
                SET
                    locked_until = @locked_until,
                    locked_by = @locked_by,
                    execute_after = @execute_after
        ";
        private const string DELETE_TASK = @"DELETE FROM @table_name WHERE name = @name";
        
        private readonly ITransactionFactory _transactionFactory;
        private readonly PostgresOptions _options;
        private readonly IClock _clock;
        private readonly IConfigurationSerializer _configurationSerializer;
        private readonly ITaskInstanceFactory _taskInstanceFactory;

        public PostgresRepository(ITransactionFactory transactionFactory, PostgresOptions options, IClock clock, IConfigurationSerializer configurationSerializer, ITaskInstanceFactory taskInstanceFactory)
        {
            _transactionFactory = transactionFactory ?? throw new ArgumentNullException(nameof(transactionFactory));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _configurationSerializer = configurationSerializer ?? throw new ArgumentNullException(nameof(configurationSerializer));
            _taskInstanceFactory = taskInstanceFactory ?? throw new ArgumentNullException(nameof(taskInstanceFactory));
        }

        public async Task CommitTaskAsync(ITask task, ILease lease, CancellationToken cancellationToken)
        {
            DateTimeOffset now = _clock.UtcNow;
            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);
            var (executeAfter, lockedUntil, lockedBy) = await FetchTaskAsync(transaction, task, cancellationToken);

            if (lease.Equals(lockedBy))
            {
                await UpdateRecord(transaction, task, task.Configuration.Schedule.GetNextExecution(now, executeAfter), null, null, cancellationToken);
            } else {
                Console.WriteLine("oops");
            }

            await transaction.CommitAsync(cancellationToken);
        }

        public async IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(IDynamicTask task, CancellationToken cancellationToken)
        {
            DateTimeOffset now = _clock.UtcNow;
            List<ITask> tasks = new List<ITask>();
            do
            {
                tasks.Clear();
                cancellationToken.ThrowIfCancellationRequested();

                var lockedBy = Lease.CreateNew();
                await using (var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken))
                {
                    using (var cmd = new NpgsqlCommand(POLL_DYNAMIC_TASK.Replace("@table_name", _options.TableName), transaction.Connection))
                    {
                        cmd.Parameters.AddWithValue("parent_name", task.GetCanonicalName());
                        cmd.Parameters.AddWithValue("now", now);
                        cmd.Parameters.AddWithValue("locked_by", lockedBy.LeaseId);
                        cmd.Parameters.AddWithValue("locked_until", now + TimeSpan.FromSeconds(60));
                        cmd.Parameters.AddWithValue("limit", 100);

                        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            var taskSpec = reader.GetFieldValue<JObject>(0);
                            var (name, taskConfiguration) = _configurationSerializer.Deserialize(taskSpec);
                            tasks.Add(_taskInstanceFactory.CreateTaskInstance(task, name, taskConfiguration));
                        }
                    }
                    await transaction.CommitAsync(cancellationToken);
                }
                foreach (var taskInstance in tasks)
                {
                    yield return (taskInstance, lockedBy);
                }
            } while (tasks.Count > 0 );
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);
            using (var cmd = new NpgsqlCommand(CREATE_TABLE.Replace("@table_name", _options.TableName), transaction.Connection))
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            using (var cmd = new NpgsqlCommand(CREATE_INDEX.Replace("@table_name", _options.TableName), transaction.Connection))
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
        }

        private async Task<(DateTimeOffset?, DateTimeOffset?, Lease?)> FetchTaskAsync(ITransaction transaction, ITask task, CancellationToken cancellationToken)
        {
            DateTimeOffset? executeAfter = null;
            DateTimeOffset? lockedUntil = null;
            Lease? lockedBy = null;

            using (var cmd = new NpgsqlCommand(FETCH_TASK.Replace("@table_name", _options.TableName), transaction.Connection))
            {
                cmd.Parameters.AddWithValue("name", task.GetCanonicalName());
                await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        if (!reader.IsDBNull(0))
                            executeAfter = reader.GetDateTime(0);
                        if (!reader.IsDBNull(1))
                            lockedUntil = reader.GetDateTime(1);
                        if (!reader.IsDBNull(2))
                            lockedBy = Lease.FromLeaseId(reader.GetString(2));
                    }
                }
            }

            return (executeAfter, lockedUntil, lockedBy);
        }

        public async Task<PollResponse> PollTaskAsync(ITask task, ILease? lease, CancellationToken cancellationToken)
        {
        
            DateTimeOffset now = _clock.UtcNow;
            Lease? receivedLease = (Lease?)lease;

            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);

            bool needsUpdate = false;
            var (executeAfter, lockedUntil, lockedBy) = await FetchTaskAsync(transaction, task, cancellationToken);
            if (!executeAfter.HasValue)
            {
                executeAfter = task.Configuration.Schedule.GetNextExecution(now, null);
                needsUpdate = true;
            }

            DateTimeOffset? scheduledAt = executeAfter;

            ResultType result;
            if (!executeAfter.HasValue)
            {
                result = ResultType.Missing;
            }
            else if (lockedUntil.HasValue && lockedUntil.Value > now && !(receivedLease?.LeaseId == lockedBy?.LeaseId))
            {
                result = ResultType.Locked;
            }
            else if (executeAfter.Value <= now && task.Configuration.ExecutionMode == ExecutionMode.AtMostOnce)
            {
                result = ResultType.Ready;
                executeAfter = task.Configuration.Schedule.GetNextExecution(now, executeAfter);
                lockedUntil = null;
                lockedBy = null;
                needsUpdate = true;
            }
            else if (executeAfter.Value <= now && task.Configuration.ExecutionMode == ExecutionMode.AtLeastOnce)
            {
                result = ResultType.Ready;
                lockedUntil = now + task.Configuration.LeaseDuration;
                lockedBy = Lease.CreateNew();
                needsUpdate = true;
            }
            else
            {
                result = ResultType.Pending;
            }

            if (needsUpdate)
            {
                await UpdateRecord(transaction, task, executeAfter, lockedUntil, lockedBy, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            return new PollResponse
            {
                Result = result,
                Lease = lockedBy,
                ScheduledAt = scheduledAt,
            };
        }

        private async Task UpdateRecord(ITransaction transaction, ITask task, DateTimeOffset? executeAfter, DateTimeOffset? lockedUntil, Lease? lockedBy, CancellationToken cancellationToken)
        {
            if (executeAfter.HasValue)
            {
                using var cmd = new NpgsqlCommand(UPDATE_TASK.Replace("@table_name", _options.TableName), transaction.Connection);
                cmd.Parameters.AddWithValue("name", task.GetCanonicalName());
                cmd.Parameters.AddWithValue("execute_after", (object?)executeAfter ?? DBNull.Value);
                cmd.Parameters.AddWithValue("locked_until", (object?)lockedUntil ?? DBNull.Value);
                cmd.Parameters.AddWithValue("locked_by", (object?)lockedBy?.LeaseId ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                using var cmd = new NpgsqlCommand(DELETE_TASK.Replace("@table_name", _options.TableName), transaction.Connection);
                cmd.Parameters.AddWithValue("name", task.GetCanonicalName());
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task RegisterTaskAsync(ITask task, CancellationToken cancellationToken)
        {
            DateTimeOffset now = _clock.UtcNow;
            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);
            using (var cmd = new NpgsqlCommand(REGISTER_TASK.Replace("@table_name", _options.TableName), transaction.Connection))
            {
                var nextExecution = task.Configuration.Schedule.GetNextExecution(now, null);
                cmd.Parameters.AddWithValue("name", task.GetCanonicalName());
                cmd.Parameters.AddWithValue("parent_name", task.ParentTask.GetCanonicalName());
                cmd.Parameters.Add(new NpgsqlParameter("task_spec", NpgsqlDbType.Json) { Value = _configurationSerializer.Serialize(task.Name, task.Configuration) });
                cmd.Parameters.AddWithValue("execute_after", (object?)nextExecution ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task UnlockTaskAsync(ITask task, ILease lease, CancellationToken cancellationToken)
        {
            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);
            var (executeAfter, lockedUntil, lockedBy) = await FetchTaskAsync(transaction, task, cancellationToken);

            if (lease.Equals(lockedBy))
            {
                await UpdateRecord(transaction, task, executeAfter, null, null, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task UnregistrerTaskAsync(ITask task, CancellationToken cancellationToken)
        {
            await using var transaction = await _transactionFactory.CreateTransactionAsync(cancellationToken);
            using (var cmd = new NpgsqlCommand(DELETE_TASK.Replace("@table_name", _options.TableName), transaction.Connection))
            {
                cmd.Parameters.AddWithValue("name", task.GetCanonicalName());
                await cmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            ;
        }
    }
}