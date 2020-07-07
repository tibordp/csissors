using Csissors.Middleware;
using Csissors.Parameters;
using Csissors.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                expression = Expression.Lambda<Func<Task>>(
                    InlineLambdaVisitor.InlineLambda<Func<IMiddleware, ITaskContext, Func<Task>, Task>>(
                        (middleware, context, next) => middleware.ExecuteAsync(context, next),
                        Expression.Constant(middleware),
                        contextParameter,
                        expression
                    )
                );
            }
            return expression;
        }

        public static IEnumerable<Expression> MapParameters(MethodInfo methodInfo, IServiceProvider serviceProvider, ParameterExpression contextParameter)
        {
            var attributeMappers = serviceProvider.GetServices<IParameterMapper>();

            foreach (var parameter in methodInfo.GetParameters())
            {
                bool mapperFound = false;
                foreach (var attributeMapper in attributeMappers.Reverse())
                {
                    var expression = attributeMapper.MapParameter(methodInfo, parameter, contextParameter);
                    if (expression != default)
                    {
                        yield return expression;
                        mapperFound = true;
                        break;
                    }
                }
                if (!mapperFound)
                {
                    throw new Exception("Could not map parameter");
                }
            }
        }
    }
}