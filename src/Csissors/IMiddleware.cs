using Csissors.Tasks;
using System;
using System.Threading.Tasks;

namespace Csissors
{
    public interface IMiddleware
    {
        Task ExecuteAsync(ITaskContext context, Func<Task> next);
    }
}