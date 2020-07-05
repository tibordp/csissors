using Csissors.Schedule;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class ControllerTaskFactory : ITaskFactory
    {
        private readonly Type _controllerType;
        private readonly MethodInfo _methodInfo;
        private readonly ISchedule _schedule;
        private readonly string _name;
        private readonly FailureMode _failureMode;
        private readonly ExecutionMode _executionMode;
        private readonly bool _dynamic;

        public ControllerTaskFactory(Type controllerType, MethodInfo methodInfo, ISchedule schedule, string name, FailureMode failureMode, ExecutionMode executionMode, bool dynamic)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            _controllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _name = name;
            _failureMode = failureMode;
            _executionMode = executionMode;
            _dynamic = dynamic;
        }

        public ICsissorsTask Build(IServiceProvider serviceProvider)
        {
            var middlewares = serviceProvider.GetServices<IMiddleware>().ToArray();
            var contextParameter = Expression.Parameter(typeof(ITaskContext));
            var expression = Expression.Lambda<Func<Task>>(
                Expression.Call(
                    _methodInfo.IsStatic
                        ? null
                        : Expression.Constant(serviceProvider.GetRequiredService(_controllerType)),
                    _methodInfo,
                    contextParameter
                )
            );

            var invokeMiddlewareMethodInfo = typeof(IMiddleware).GetMethod(nameof(IMiddleware.ExecuteAsync));
            foreach (var middleware in middlewares.Reverse())
            {
                expression = Expression.Lambda<Func<Task>>(
                    Expression.Call(
                        Expression.Constant(middleware),
                        invokeMiddlewareMethodInfo,
                        contextParameter,
                        expression
                    )
                );
            }

            var outer = Expression.Lambda<TaskFunc>(Expression.Invoke(expression), contextParameter);
            return new DelegateTask(outer.Compile(), _schedule, _name, _failureMode, _executionMode, _dynamic);
        }
    }

}