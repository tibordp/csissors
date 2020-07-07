using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class TaskContainerTaskBuilder : ITaskBuilder
    {
        private readonly Type _taskContainerType;
        private readonly MethodInfo _methodInfo;
        private readonly string _name;
        private readonly TaskConfiguration? _configuration;

        public TaskContainerTaskBuilder(Type taskContainerType, MethodInfo methodInfo, string name, TaskConfiguration? configuration = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            _taskContainerType = taskContainerType ?? throw new ArgumentNullException(nameof(taskContainerType));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _configuration = configuration;
            _name = name;
        }

        public ITask BuildStatic(IServiceProvider serviceProvider)
        {
            if (_configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            return new DelegateTask(CreateCallDelegate(serviceProvider), _name, _configuration);
        }

        public IDynamicTask BuildDynamic(IServiceProvider serviceProvider)
        {
            return new DelegateTask(CreateCallDelegate(serviceProvider), _name, null);
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
                    TaskBuilderUtils.MapParameters(_methodInfo, serviceProvider, contextParameter)
                )
            ));
            var taskFuncExpression = Expression.Lambda<TaskFunc>(expression.Body, contextParameter);
            Console.WriteLine(taskFuncExpression);

            return taskFuncExpression.Compile();
        }
    }
}