using Csissors.Attributes;
using Csissors.Reflection;
using Csissors.Tasks;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Csissors.Parameters
{
    public class TaskDataParameterMapper : IParameterMapper
    {
        private static T? Convert<T>(ITaskContext context, string key) where T : class
        {
            if (context.Task.Configuration.Data.TryGetValue(key, out var obj))
            {
                return (T?)obj;
            }
            return default;
        }

        public Expression? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo, ParameterExpression contextParameter)
        {
            var attribute = parameterInfo.GetCustomAttribute<FromTaskDataAttribute>();
            if (attribute != null)
            {
                var keyExpression = Expression.Constant(attribute.Name ?? parameterInfo.Name);
                if (attribute.Optional)
                {
                    var convertMethod = typeof(TaskDataParameterMapper)
                        .GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(parameterInfo.ParameterType);
                    return Expression.Call(null, convertMethod, contextParameter, keyExpression);
                }
                return Expression.Convert(
                    InlineLambdaVisitor.InlineLambda<Func<ITaskContext, string, object?>>(
                        (ctx, attributeName) => ctx.Task.Configuration.Data[attributeName],
                        contextParameter,
                        keyExpression
                    ), parameterInfo.ParameterType
                );
            }
            return default;
        }
    }
}