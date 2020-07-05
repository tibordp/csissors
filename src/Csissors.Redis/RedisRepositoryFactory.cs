using Csissors.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Csissors.Redis
{

    public class RedisRepositoryFactory : IRepositoryFactory
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConfigurationOptions _configurationOptions;

        public RedisRepositoryFactory(ILoggerFactory loggerFactory, IOptions<RedisOptions> configurationOptions)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _log = loggerFactory.CreateLogger<RedisRepositoryFactory>();
            _configurationOptions = configurationOptions.Value.ConfigurationOptions;
        }

        public async Task<IRepository> CreateRepositoryAsync()
        {
            _log.LogInformation("Connecting to Redis");
            IConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(_configurationOptions);
            RedisRepository repository = new RedisRepository(_loggerFactory, redis);
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