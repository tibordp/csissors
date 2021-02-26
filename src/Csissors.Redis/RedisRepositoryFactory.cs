using Csissors.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Redis
{

    public class RedisRepositoryFactory : IRepositoryFactory
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;
        private readonly RedisOptions _options;

        public RedisRepositoryFactory(ILoggerFactory loggerFactory, IOptions<RedisOptions> options)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _log = loggerFactory.CreateLogger<RedisRepositoryFactory>();
            _options = options.Value;
        }

        public async Task<IRepository> CreateRepositoryAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Connecting to Redis");
            IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions);
            RedisRepository repository = new RedisRepository(_loggerFactory, redis, _options.KeyPrefix);
            try
            {
                await Task.Yield();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error while initializing connection");
                await redis.CloseAsync();
                throw;
            }
            return repository;
        }
    }
}