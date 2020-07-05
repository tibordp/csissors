using System.Threading;

namespace Csissors.Tasks
{
    public interface ITaskContext
    {
        IAppContext AppContext { get; }
        CancellationToken Cancellation { get; }
        ICsissorsTask Task { get; }
    }
}