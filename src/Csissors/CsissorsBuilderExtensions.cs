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

        #region AddDynamicTask
        public static CsissorsBuilder AddDynamicTask(this CsissorsBuilder builder, string taskName, Func<Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1>(this CsissorsBuilder builder, string taskName, Func<T1, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2>(this CsissorsBuilder builder, string taskName, Func<T1, T2, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        public static CsissorsBuilder AddDynamicTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this CsissorsBuilder builder, string taskName, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> taskFunc) => builder.AddDynamicTask(taskName, taskFunc);
        #endregion

        #region AddTask
        public static CsissorsBuilder AddTask(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        public static CsissorsBuilder AddTask<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this CsissorsBuilder builder, string taskName, TaskConfiguration taskConfiguration, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> taskFunc) => builder.AddTask(taskName, taskConfiguration, taskFunc);
        #endregion  
    }

}