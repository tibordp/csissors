using Csissors.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

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

            var duplicateTaskName = tasks.Select(task => task.GetCanonicalName())
                .Concat(dynamicTasks.Select(task => task.GetCanonicalName()))
                .GroupBy(key => key)
                .Where(grouping => grouping.Count() > 1)
                .FirstOrDefault();

            if (duplicateTaskName != null)
            {
                throw new InvalidOperationException($"Duplicate task name \"{duplicateTaskName.Key}\"");
            }

            return new TaskSet(tasks, dynamicTasks);
        }
    }

}