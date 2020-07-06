using Csissors.Middleware;
using Csissors.Repository;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Csissors
{
    public static class CsissorsBuilderExtensions
    {
        public static CsissorsBuilder ConfigureServices(this CsissorsBuilder builder, Action<IServiceCollection> callback)
        {
            callback(builder.Services);
            return builder;
        }

        public static CsissorsBuilder AddMiddleware<T>(this CsissorsBuilder builder) where T : class, IMiddleware
        {
            builder.Services.AddSingleton<IMiddleware, T>();
            return builder;
        }

        public static CsissorsBuilder AddMiddleware<T>(this CsissorsBuilder builder, Func<IServiceProvider, T> factory) where T : class, IMiddleware
        {
            builder.Services.AddSingleton<IMiddleware, T>(factory);
            return builder;
        }

        public static CsissorsBuilder AddMiddleware(this CsissorsBuilder builder, Func<ITaskContext, Func<Task>, Task> @delegate)
        {
            builder.Services.AddSingleton<IMiddleware>(_ => new DelegateMiddleware(@delegate));
            return builder;
        }

        public static CsissorsBuilder AddInMemoryRepository(this CsissorsBuilder builder)
        {
            builder.Services.AddSingleton<IRepositoryFactory, InMemoryRepository>();
            return builder;
        }
    }

}