using Csissors.Middleware;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Csissors.Tasks
{
    internal static class TaskBuilderUtils
    {
        public static Expression<Func<Task>> ApplyMiddlewares(IServiceProvider serviceProvider, ParameterExpression contextParameter, Expression<Func<Task>> expression)
        {
            var middlewares = serviceProvider.GetServices<IMiddleware>().ToArray();
            foreach (var middleware in middlewares.Reverse())
            {
                var invokeMiddlewareMethodInfo = middleware.GetType().GetMethod(nameof(IMiddleware.ExecuteAsync));
                expression = Expression.Lambda<Func<Task>>(
                    Expression.Call(
                        Expression.Constant(middleware),
                        invokeMiddlewareMethodInfo,
                        contextParameter,
                        expression
                    )
                );
            }
            return expression;
        }
    }
}