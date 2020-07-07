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
        public Expression? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo, ParameterExpression contextParameter)
        {
            var attribute = parameterInfo.GetCustomAttribute<FromTaskDataAttribute>();
            if (attribute != null)
            {
                return Expression.Convert(
                    InlineLambdaVisitor.InlineLambda<Func<ITaskContext, string, object?>>(
                        (ctx, attributeName) => ctx.Task.Configuration.Data[attributeName],
                        contextParameter,
                        Expression.Constant(attribute.Name ?? parameterInfo.Name)
                    ), parameterInfo.ParameterType
                );
            }
            return default;
        }
    }
}