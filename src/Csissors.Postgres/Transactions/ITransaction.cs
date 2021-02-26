using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres.Transactions
{
    public interface ITransaction: IAsyncDisposable {
        NpgsqlConnection Connection { get; }
        Task CommitAsync(CancellationToken cancellationToken);
    }
}