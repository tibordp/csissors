using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres.Transactions
{
    internal class TransactionFactory : ITransactionFactory
    {
        private class Transaction : ITransaction
        {
            private readonly NpgsqlTransaction _transaction;
            public NpgsqlConnection Connection { get; }

            public Transaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
            {
                Connection = connection ?? throw new ArgumentNullException(nameof(connection));
                _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            }

            public Task CommitAsync(CancellationToken cancellationToken) => _transaction.CommitAsync(cancellationToken);

            public async ValueTask DisposeAsync()
            {
                await using (Connection)
                await using (_transaction)
                    ;
            }
        }

        private readonly string _connectionString;

        public TransactionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<ITransaction> CreateTransactionAsync(CancellationToken cancellationToken)
        {
            var connection = new NpgsqlConnection(_connectionString);
            try {
                await connection.OpenAsync(cancellationToken);
                connection.TypeMapper.UseJsonNet();
                return new Transaction(connection, connection.BeginTransaction());
            } catch {
                await connection.DisposeAsync();
                throw;
            }
        }
    }
}