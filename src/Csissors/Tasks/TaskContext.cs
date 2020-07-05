using System.Threading;

namespace Csissors.Tasks
{
    internal class TaskContext : ITaskContext
    {
        public IAppContext AppContext { get; internal set; }
        public CancellationToken Cancellation { get; internal set; }
        public ICsissorsTask Task { get; internal set; }
    }
}