using Csissors.Reflection;
using Csissors.Tasks;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Csissors.Parameters
{
    public abstract class ParameterMapper<T> : IParameterMapper
    {
        public abstract Expression<Func<ITaskContext, T>>? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo);
        Expression? IParameterMapper.MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo, ParameterExpression contextParameter)
        {
            if (parameterInfo.ParameterType == typeof(T))
            {
                var typedResult = MapParameter(taskMethodInfo, parameterInfo);
                if (typedResult != default)
                {
                    return InlineLambdaVisitor.InlineLambda(
                        typedResult,
                        contextParameter
                    );
                }
            }
            return null;
        }
    }
}