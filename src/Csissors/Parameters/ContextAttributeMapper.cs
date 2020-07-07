using Csissors.Tasks;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Csissors.Parameters
{
    public class ContextAttributeMapper : ParameterMapper<ITaskContext>
    {
        public override Expression<Func<ITaskContext, ITaskContext>>? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo) => (context) => context;
    }
}