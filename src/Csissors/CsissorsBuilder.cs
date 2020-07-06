using Cronos;
using Csissors.Repository;
using Csissors.Schedule;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors
{
    public class CsissorsBuilder
    {
        private readonly List<ITaskBuilder> _tasks = new List<ITaskBuilder>();
        public IServiceCollection Services { get; } = new ServiceCollection();

        public CsissorsBuilder()
        {
            Services.AddOptions();
        }

        public CsissorsBuilder AddTaskContainer<T>() where T : class
        {
            var staticMethods = typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            foreach (var methodInfo in staticMethods)
            {
                if (TryCreateTaskFactory(typeof(T), methodInfo, out var task))
                {
                    _tasks.Add(task);
                }
            }
            Services.AddSingleton<T>();
            return this;
        }

        public async Task<IAppContext> BuildAsync()
        {
            var serviceProvider = Services.BuildServiceProvider();
            var tasks = CompileTasks(serviceProvider);
            var repositoryFactory = serviceProvider.GetRequiredService<IRepositoryFactory>();
            var options = serviceProvider.GetRequiredService<IOptions<CsissorsOptions>>();
            var repository = await repositoryFactory.CreateRepositoryAsync();
            return new CsissorsContext(repository, tasks, options);
        }

        private IReadOnlyList<ITask> CompileTasks(IServiceProvider serviceProvider)
        {
            return _tasks.Select(task => task.Build(serviceProvider)).ToArray();
        }

        private static bool TryCreateTaskFactory(Type taskContainerType, MethodInfo methodInfo, out ITaskBuilder result)
        {
            result = null;

            CsissorsTaskAttribute attribute = methodInfo.GetCustomAttribute<CsissorsTaskAttribute>();
            if (attribute != null)
            {
                ISchedule schedule;
                if (attribute.Schedule != null)
                {
                    CronExpression cronExpression = CronExpression.Parse(attribute.Schedule);
                    TimeZoneInfo timeZoneInfo = attribute.TimeZone != null
                        ? TimeZoneInfo.FindSystemTimeZoneById(attribute.TimeZone)
                        : TimeZoneInfo.Utc;

                    schedule = new CronSchedule(cronExpression, timeZoneInfo, attribute.FastForward);
                }
                else
                {
                    schedule = new IntervalSchedule(
                        new TimeSpan(attribute.Days, attribute.Hours, attribute.Minutes, attribute.Seconds),
                        attribute.FastForward
                    );
                }
                var taskName = attribute.Name ?? methodInfo.Name;
                var taskConfiguration = new TaskConfiguration(schedule, attribute.FailureMode, attribute.ExecutionMode, attribute.Dynamic);
                result = new TaskContainerTaskBuilder(taskContainerType, methodInfo, taskName, taskConfiguration);
                return true;
            }

            return false;
        }
    }

}