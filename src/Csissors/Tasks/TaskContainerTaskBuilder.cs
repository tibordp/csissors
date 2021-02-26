using Cronos;
using Csissors.Attributes;
using Csissors.Schedule;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class TaskContainerTaskBuilder : ITaskBuilder
    {
        private readonly Type _taskContainerType;
        private readonly MethodInfo _methodInfo;
        private readonly Attribute _attribute;

        public TaskContainerTaskBuilder(Type taskContainerType, MethodInfo methodInfo, Attribute attribute)
        {
            _taskContainerType = taskContainerType ?? throw new ArgumentNullException(nameof(taskContainerType));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }

        public ITask BuildStatic(IServiceProvider serviceProvider)
        {
            if (_attribute is CsissorsTaskAttribute attribute)
            {
                var options = serviceProvider.GetRequiredService<IOptions<CsissorsOptions>>().Value;
                var (name, taskConfiguration) = CreateTaskConfiguration(attribute, options.DefaultLeaseDuration);
                return new DelegateTask(CreateCallDelegate(serviceProvider), name, taskConfiguration);
            }
            throw new ArgumentOutOfRangeException(nameof(attribute));
        }

        public IDynamicTask BuildDynamic(IServiceProvider serviceProvider)
        {
            if (_attribute is CsissorsDynamicTaskAttribute attribute)
            {
                var name = attribute.Name ?? _methodInfo.Name;
                return new DelegateTask(CreateCallDelegate(serviceProvider), name, null);
            }
            throw new ArgumentOutOfRangeException(nameof(attribute));
        }

        private (string name, TaskConfiguration taskConfiguration) CreateTaskConfiguration(CsissorsTaskAttribute taskAttribute, TimeSpan leaseDuration)
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
            var taskName = taskAttribute.Name ?? _methodInfo.Name;
            var data = new Dictionary<string, object?>();
            return (taskName, new TaskConfiguration(
                schedule,
                taskAttribute.FailureMode,
                taskAttribute.ExecutionMode,
                leaseDuration,
                data
            ));
        }

        private TaskFunc CreateCallDelegate(IServiceProvider serviceProvider)
        {
            var contextParameter = Expression.Parameter(typeof(ITaskContext));
            var expression = TaskBuilderUtils.ApplyMiddlewares(serviceProvider, contextParameter, Expression.Lambda<Func<Task>>(
                Expression.Call(
                    _methodInfo.IsStatic
                        ? null
                        : Expression.Constant(serviceProvider.GetRequiredService(_taskContainerType)),
                    _methodInfo,
                    TaskBuilderUtils.MapParameters(serviceProvider, contextParameter, _methodInfo)
                )
            ));
            var taskFuncExpression = Expression.Lambda<TaskFunc>(expression.Body, contextParameter);
            Console.WriteLine(taskFuncExpression);

            return taskFuncExpression.Compile();
        }
    }
}