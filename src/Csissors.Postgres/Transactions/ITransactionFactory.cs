using System.Threading;
using System.Threading.Tasks;

namespace Csissors.Postgres.Transactions
{
    public interface ITransactionFactory {
        Task<ITransaction> CreateTransactionAsync(CancellationToken cancellationToken);
    }
}