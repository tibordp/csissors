using Csissors.Repository;
using Csissors.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Redis
{
    public class RedisRepository : IRepository
    {
        private readonly ILogger _log;

        private readonly IConnectionMultiplexer _redis;
        private readonly string _keyspace;
        private readonly IDatabaseAsync _database;
        private readonly string _manageScript;
        private readonly string _pollDynamicScript;

        private static string ParseLuaScript(string scriptName)
        {
            var assembly = typeof(RedisRepository).Assembly;
            var provider = new EmbeddedFileProvider(assembly);
            using (var reader = new StreamReader(provider.GetFileInfo(scriptName).CreateReadStream()))
            {
                return reader.ReadToEnd();
            }
        }

        public RedisRepository(ILoggerFactory loggerFactory, IConnectionMultiplexer redis, string keyspace)
        {
            _log = loggerFactory.CreateLogger<RedisRepository>();
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _keyspace = keyspace;
            _manageScript = ParseLuaScript("Scripts.manage.lua");
            _pollDynamicScript = ParseLuaScript("Scripts.poll_dynamic.lua");
            _database = _redis.GetDatabase();
        }

        public Task CommitTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            await _redis.CloseAsync();
        }

        public IAsyncEnumerable<(ITask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, IDynamicTask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PollResponse> PollTaskAsync(DateTimeOffset now, ITask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
    }
}