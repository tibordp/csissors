using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal class DelegateTaskBuilder : ITaskBuilder
    {
        private readonly Delegate _delegate;
        private readonly string _name;
        private readonly TaskConfiguration? _configuration;

        public DelegateTaskBuilder(Delegate @delegate, string name, TaskConfiguration? configuration = null)
        {
            _delegate = @delegate;
            _name = name;
            _configuration = configuration;
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
                    _delegate.Method.IsStatic
                        ? null
                        : Expression.Constant(_delegate.Target),
                    _delegate.Method,
                    TaskBuilderUtils.MapParameters(serviceProvider, contextParameter, _delegate.Method)
                )
            ));
            var taskFuncExpression = Expression.Lambda<TaskFunc>(expression.Body, contextParameter);
            Console.WriteLine(taskFuncExpression);

            return taskFuncExpression.Compile();
        }
    }
}