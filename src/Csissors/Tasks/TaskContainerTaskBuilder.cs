using Csissors.Schedule;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    class InlineLambdaVisitor<T> : ExpressionVisitor
    {
        private readonly Expression[] _replacements;
        private ReadOnlyCollection<ParameterExpression>? _parameters;

        public InlineLambdaVisitor(params Expression[] replacements)
        {
            _replacements = replacements;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (_parameters == null)
            {
                if (node.Parameters.Count != _replacements.Length)
                {
                    throw new ArgumentException("Parameter count mismatch");
                }
                _parameters = node.Parameters;
                return Visit(node.Body);
            }
            else
            {
                return base.VisitLambda(node);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parameters != null)
            {
                var parameterIndex = _parameters.IndexOf(node);
                if (parameterIndex != -1)
                {
                    return _replacements[parameterIndex];
                }
            }
            return base.VisitParameter(node);
        }

        public static Expression InlineLambda(Expression<T> node, params Expression[] replacements)
        {
            return new InlineLambdaVisitor<T>(replacements).VisitLambda(node);
        }
    }

    internal class TaskContainerTaskBuilder : ITaskBuilder
    {
        private readonly Type _taskContainerType;
        private readonly MethodInfo _methodInfo;
        private readonly string _name;
        private readonly TaskConfiguration _configuration;

        public TaskContainerTaskBuilder(Type taskContainerType, MethodInfo methodInfo, string name, TaskConfiguration configuration)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            _taskContainerType = taskContainerType ?? throw new ArgumentNullException(nameof(taskContainerType));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _name = name;
        }

        public ITask Build(IServiceProvider serviceProvider)
        {
            var contextParameter = Expression.Parameter(typeof(ITaskContext));

            var expression = TaskBuilderUtils.ApplyMiddlewares(serviceProvider, contextParameter, Expression.Lambda<Func<Task>>(
                Expression.Call(
                    _methodInfo.IsStatic
                        ? null
                        : Expression.Constant(serviceProvider.GetRequiredService(_taskContainerType)),
                    _methodInfo,
                    MapParameters(serviceProvider, contextParameter)
                )
            ));
            Console.WriteLine(expression.ToString());
            var outer = Expression.Lambda<TaskFunc>(Expression.Invoke(expression), contextParameter);
            return new DelegateTask(outer.Compile(), _name, _configuration);
        }

        private IEnumerable<Expression> MapParameters(IServiceProvider serviceProvider, ParameterExpression contextParameter)
        {
            foreach (var parameter in _methodInfo.GetParameters())
            {
                var attribute = parameter.GetCustomAttribute<FromTaskDataAttribute>();

                switch (parameter)
                {
                    case ParameterInfo _ when parameter.ParameterType == typeof(ITaskContext):
                        yield return contextParameter;
                        break;
                    case ParameterInfo _ when attribute != null:
                        var accessor = InlineLambdaVisitor<Func<ITaskContext, string, object?>>.InlineLambda(
                            (ctx, attributeName) => ctx.Task.Configuration.Data[attributeName],
                            contextParameter,
                            Expression.Constant(attribute.Name)
                        );
                        yield return Expression.Convert(
                            accessor, parameter.ParameterType
                        );
                        break;
                    default:
                        yield return Expression.Constant(serviceProvider.GetRequiredService(parameter.ParameterType));
                        break;
                }
            }
        }
    }
}