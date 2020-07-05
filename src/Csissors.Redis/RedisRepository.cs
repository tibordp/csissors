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
        private readonly LuaScript _manageScript;
        private readonly LuaScript _pollDynamicScript;

        private static LuaScript ParseLuaScript(string scriptName)
        {
            var assembly = typeof(RedisRepository).Assembly;
            var provider = new EmbeddedFileProvider(assembly);
            using (var reader = new StreamReader(provider.GetFileInfo(scriptName).CreateReadStream()))
            {
                string scriptText = reader.ReadToEnd();
                Console.WriteLine(scriptText);
                return LuaScript.Prepare(scriptText);
            }
        }

        public RedisRepository(ILoggerFactory loggerFactory, IConnectionMultiplexer redis)
        {
            _log = loggerFactory.CreateLogger<RedisRepository>();
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _manageScript = ParseLuaScript("Scripts.manage.lua");
            _pollDynamicScript = ParseLuaScript("Scripts.poll_dynamic.lua");
        }

        public Task CommitTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async ValueTask DisposeAsync()
        {
            await _redis.CloseAsync();
        }

        public IAsyncEnumerable<(ICsissorsTask, ILease)> PollDynamicTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PollResponse> PollTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RegisterTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UnlockTask(DateTimeOffset now, ICsissorsTask task, ILease lease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UnregistrerTaskAsync(DateTimeOffset now, ICsissorsTask task, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}