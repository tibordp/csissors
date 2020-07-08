using Csissors.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Csissors.Postgres
{

    public class PostgresRepositoryFactory : IRepositoryFactory
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;
        private readonly PostgresOptions _options;

        public PostgresRepositoryFactory(ILoggerFactory loggerFactory, IOptions<PostgresOptions> options)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _log = loggerFactory.CreateLogger<PostgresRepositoryFactory>();
            _options = options.Value;
        }

        public async Task<IRepository> CreateRepositoryAsync()
        {
            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            var foo = new PostgresRepository(conn, _options);
            try
            {
                await foo.Initialize();
                return foo;
            }
            catch
            {
                await foo.DisposeAsync();
                throw;
            }
        }
    }
}