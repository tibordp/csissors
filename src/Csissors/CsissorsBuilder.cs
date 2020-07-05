using Cronos;
using Csissors.Repository;
using Csissors.Schedule;
using Csissors.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors
{
    public class CsissorsBuilder
    {
        private readonly List<ITaskFactory> _tasks = new List<ITaskFactory>();
        public IServiceCollection Services { get; } = new ServiceCollection();

        public CsissorsBuilder AddController<T>() where T : class
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
            var repository = await repositoryFactory.CreateRepositoryAsync();
            return new CsissorsContext(repository, tasks);
        }

        private IReadOnlyList<ICsissorsTask> CompileTasks(IServiceProvider serviceProvider)
        {
            return _tasks.Select(task => task.Build(serviceProvider)).ToArray();
        }

        private static bool TryCreateTaskFactory(Type controllerType, MethodInfo methodInfo, out ITaskFactory result)
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
                result = new ControllerTaskFactory(controllerType, methodInfo, schedule, taskName, attribute.FailureMode, attribute.ExecutionMode, attribute.Dynamic);
                return true;
            }

            return false;
        }
    }

}