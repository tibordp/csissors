using Microsoft.Extensions.DependencyInjection;
using System;

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
    }

}