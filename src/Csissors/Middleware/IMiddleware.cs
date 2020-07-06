using Csissors.Tasks;
using System;
using System.Threading.Tasks;

namespace Csissors.Middleware
{
    public interface IMiddleware
    {
        Task ExecuteAsync(ITaskContext context, Func<Task> next);
    }
}