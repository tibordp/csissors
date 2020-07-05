using System;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public interface IAppContext : IAsyncDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}