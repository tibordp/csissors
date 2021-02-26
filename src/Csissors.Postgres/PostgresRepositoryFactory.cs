using Csissors.Postgres.Transactions;
using Csissors.Repository;
using Csissors.Serialization;
using Csissors.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres
{
    public class PostgresRepositoryFactory : IRepositoryFactory
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IClock _clock;
        private readonly IConfigurationSerializer _configurationSerializer;
        private readonly ITaskInstanceFactory _taskInstanceFactory;
        private readonly PostgresOptions _options;

        public PostgresRepositoryFactory(ILoggerFactory loggerFactory, IOptions<PostgresOptions> options, IClock clock, IConfigurationSerializer configurationSerializer, ITaskInstanceFactory taskInstanceFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _configurationSerializer = configurationSerializer ?? throw new ArgumentNullException(nameof(configurationSerializer));
            _taskInstanceFactory = taskInstanceFactory ?? throw new ArgumentNullException(nameof(taskInstanceFactory));
            _log = loggerFactory.CreateLogger<PostgresRepositoryFactory>();
            _options = options.Value;
        }

        public async Task<IRepository> CreateRepositoryAsync(CancellationToken cancellationToken)
        {
            var transactionFactory = new TransactionFactory(_options.ConnectionString);
            var repository = new PostgresRepository(transactionFactory, _options, _clock, _configurationSerializer, _taskInstanceFactory);
            try {
                await repository.InitializeAsync(cancellationToken);
                return repository;
            } catch {
                await repository.DisposeAsync();
                throw;
            }
        }
    }
}