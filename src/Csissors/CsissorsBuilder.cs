using Csissors.Attributes;
using Csissors.Executor;
using Csissors.Parameters;
using Csissors.Repository;
using Csissors.Serialization;
using Csissors.Tasks;
using Csissors.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{

    public class CsissorsBuilder
    {
        private readonly List<ITaskBuilder> _staticTaskBuilders = new List<ITaskBuilder>();
        private readonly List<ITaskBuilder> _dynamicTaskBuilders = new List<ITaskBuilder>();
        public IServiceCollection Services { get; } = new ServiceCollection();

        public CsissorsBuilder(bool skipDefaultServices = false)
        {
            if (!skipDefaultServices)
            {
                Services
                    .AddLogging()
                    .AddOptions()
                    .AddSingleton<IExecutor, DefaultExecutor>()
                    .AddSingleton<IParameterMapper, ContextAttributeMapper>()
                    .AddSingleton<IParameterMapper, CancellationTokenAttributeMapper>()
                    .AddSingleton<IParameterMapper, ServiceAttributeMapper>()
                    .AddSingleton<IParameterMapper, TaskDataParameterMapper>()
                    .AddSingleton<ITaskInstanceFactory, DefaultTaskInstanceFactory>()
                    .AddSingleton<IClock, Clock>();
            }
        }

        public CsissorsBuilder AddTaskContainer(Type type)
        {
            var staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in staticMethods)
            {
                RegisterTaskMethod(type, methodInfo);
            }
            Services.AddSingleton(type);
            return this;
        }

        public CsissorsBuilder AddTaskContainer<T>() where T : class => AddTaskContainer(typeof(T));

        public CsissorsBuilder AddAssembly() => AddAssembly(Assembly.GetCallingAssembly());

        public CsissorsBuilder AddAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<CsissorsTaskContainerAttribute>() != null)
                {
                    AddTaskContainer(type);
                }
            }
            return this;
        }

        internal CsissorsBuilder AddTask(string taskName, TaskConfiguration taskConfiguration, Delegate taskFunc)
        {
            _staticTaskBuilders.Add(new DelegateTaskBuilder(taskFunc, taskName, taskConfiguration));
            return this;
        }

        internal CsissorsBuilder AddDynamicTask(string taskName, Delegate taskFunc)
        {
            _dynamicTaskBuilders.Add(new DelegateTaskBuilder(taskFunc, taskName));
            return this;
        }

        public async Task<IAppContext> BuildAsync(CancellationToken cancellationToken)
        {
            var serviceProvider = Services.BuildServiceProvider();
            var taskSet = TaskSet.BuildTasks(serviceProvider, _staticTaskBuilders, _dynamicTaskBuilders);
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            var repository = await repositoryFactory.CreateRepositoryAsync(cancellationToken);

            return ActivatorUtilities.CreateInstance<CsissorsContext>(serviceProvider, taskSet, repository);
        }

        private void RegisterTaskMethod(Type taskContainerType, MethodInfo methodInfo)
        {
            foreach (var attribute in methodInfo.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case CsissorsTaskAttribute _:
                        _staticTaskBuilders.Add(new TaskContainerTaskBuilder(taskContainerType, methodInfo, attribute));
                        break;
                    case CsissorsDynamicTaskAttribute _:
                        _dynamicTaskBuilders.Add(new TaskContainerTaskBuilder(taskContainerType, methodInfo, attribute));
                        break;
                }
            }
        }
    }

}