using Csissors.Repository;
using Csissors.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres
{
    public class PostgresRepository : IRepository
    {
        private const string CREATE_TABLE = @"CREATE TABLE IF NOT EXISTS @table_name (
                name text PRIMARY KEY,
                parent_name text,
                locked_until timestamptz,
                locked_by uuid,
                execute_after timestamptz,
                task_spec json
            );
            CREATE INDEX IF NOT EXISTS @table_name 
            ON @table_name  (parent_name, GREATEST(locked_until, execute_after))
        ";

        private const string POLL_DYNAMIC_TASK = @"UPDATE @table_name a
            SET
                locked_until = @locked_until,
                locked_by = @locked_by
            FROM (
                SELECT name FROM @table_name
                WHERE parent_name = @parent_name AND GREATEST(locked_until, execute_after) <= $2
                ORDER BY GREATEST(locked_until, execute_after) ASC
                LIMIT @limit
                FOR UPDATE SKIP LOCKED
            ) b
            WHERE a.name = b.name
            RETURNING *
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

        private const string POLL_TASK = @"SELECT * FROM @table_name WHERE name = @name FOR UPDATE";

        private NpgsqlConnection _connection;
        private PostgresOptions _options;

        public PostgresRepository(NpgsqlConnection conn, PostgresOptions options)
        {
            _connection = conn ?? throw new ArgumentNullException(nameof(conn));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Task CommitTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task Initialize()
        {
            await using (var transaction = _connection.BeginTransaction())
            {
                using (var cmd = new NpgsqlCommand(CREATE_TABLE.Replace("@table_name", _options.TableName), _connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                await transaction.CommitAsync();
            }
        }

        public async Task<PollResponse> PollTaskAsync(DateTimeOffset now, ITask task, ILease? lease, CancellationToken cancellationToken)
        {
            await using (var transaction = _connection.BeginTransaction())
            {
                throw new NotImplementedException();
            }
        }

        public Task RegisterTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UnlockTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UnregistrerTaskAsync(DateTimeOffset now, ITask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            await using (_connection)
                ;
        }
    }
}