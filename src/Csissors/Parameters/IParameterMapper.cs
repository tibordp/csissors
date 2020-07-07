using System.Linq.Expressions;
using System.Reflection;

namespace Csissors.Parameters
{
    public interface IParameterMapper
    {
        Expression? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo, ParameterExpression contextParameter);
    }
}