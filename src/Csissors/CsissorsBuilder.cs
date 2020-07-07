using Cronos;
using Csissors.Attributes;
using Csissors.Executor;
using Csissors.Parameters;
using Csissors.Repository;
using Csissors.Schedule;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Csissors
{
    public class TaskSet
    {
        public IReadOnlyList<ITask> StaticTasks { get; }
        public IReadOnlyList<IDynamicTask> DynamicTasks { get; }

        public TaskSet(IReadOnlyList<ITask> staticTasks, IReadOnlyList<IDynamicTask> dynamicTasks)
        {
            StaticTasks = staticTasks ?? throw new ArgumentNullException(nameof(staticTasks));
            DynamicTasks = dynamicTasks ?? throw new ArgumentNullException(nameof(dynamicTasks));
        }

        internal static TaskSet BuildTasks(IServiceProvider serviceProvider, IEnumerable<ITaskBuilder> staticTaskBuilders, IEnumerable<ITaskBuilder> dynamicTaskBuilders)
        {
            var tasks = staticTaskBuilders.Select(taskBuilder => taskBuilder.BuildStatic(serviceProvider)).ToArray();
            var dynamicTasks = dynamicTaskBuilders.Select(taskBuilder => taskBuilder.BuildDynamic(serviceProvider)).ToArray();

            return new TaskSet(tasks, dynamicTasks);
        }
    }

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
                    .AddSingleton<IParameterMapper, TaskDataParameterMapper>();
            }
        }

        public CsissorsBuilder AddTaskContainer<T>() where T : class
        {
            var staticMethods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in staticMethods)
            {
                RegisterTaskMethod(typeof(T), methodInfo);
            }
            Services.AddSingleton<T>();
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

        public async Task<IAppContext> BuildAsync()
        {
            var serviceProvider = Services.BuildServiceProvider();
            var taskSet = TaskSet.BuildTasks(serviceProvider, _staticTaskBuilders, _dynamicTaskBuilders);
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            var options = serviceProvider.GetRequiredService<IOptions<CsissorsOptions>>();
            var executor = serviceProvider.GetRequiredService<IExecutor>();
            var logger = serviceProvider.GetRequiredService<ILogger<CsissorsContext>>();
            var repository = await repositoryFactory.CreateRepositoryAsync();
            return new CsissorsContext(logger, executor, repository, taskSet, options);
        }

        private void RegisterTaskMethod(Type taskContainerType, MethodInfo methodInfo)
        {
            foreach (var attribute in methodInfo.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case CsissorsTaskAttribute taskAttribute:
                        {
                            ISchedule schedule;
                            if (taskAttribute.Schedule != null)
                            {
                                CronExpression cronExpression = CronExpression.Parse(taskAttribute.Schedule);
                                TimeZoneInfo timeZoneInfo = taskAttribute.TimeZone != null
                                    ? TimeZoneInfo.FindSystemTimeZoneById(taskAttribute.TimeZone)
                                    : TimeZoneInfo.Utc;

                                schedule = new CronSchedule(cronExpression, timeZoneInfo, taskAttribute.FastForward);
                            }
                            else
                            {
                                schedule = new IntervalSchedule(
                                    new TimeSpan(taskAttribute.Days, taskAttribute.Hours, taskAttribute.Minutes, taskAttribute.Seconds),
                                    taskAttribute.FastForward
                                );
                            }
                            var taskName = taskAttribute.Name ?? methodInfo.Name;
                            var leaseDuration = TimeSpan.FromMinutes(1);
                            var data = new Dictionary<string, object?>();
                            var taskConfiguration = new TaskConfiguration(
                                schedule,
                                taskAttribute.FailureMode,
                                taskAttribute.ExecutionMode,
                                leaseDuration,
                                data
                            );
                            _staticTaskBuilders.Add(new TaskContainerTaskBuilder(taskContainerType, methodInfo, taskName, taskConfiguration));
                            break;
                        }
                    case CsissorsDynamicTaskAttribute dynamicTaskAttribute:
                        {
                            var taskName = dynamicTaskAttribute.Name ?? methodInfo.Name;
                            _dynamicTaskBuilders.Add(new TaskContainerTaskBuilder(taskContainerType, methodInfo, taskName));
                            break;
                        }
                }
            }
        }

    }

}