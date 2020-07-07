using Csissors.Tasks;
using System;
using System.Threading.Tasks;

namespace Csissors.Middleware
{
    /// <summary>
    /// 
    /// </summary>
    internal class DelegateMiddleware : IMiddleware
    {
        private readonly Func<ITaskContext, Func<Task>, Task> _delegate;
        public DelegateMiddleware(Func<ITaskContext, Func<Task>, Task> @delegate)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }
        public Task ExecuteAsync(ITaskContext context, Func<Task> next) => _delegate(context, next);
    }
}