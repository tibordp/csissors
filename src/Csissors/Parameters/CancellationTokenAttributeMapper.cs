using Csissors.Tasks;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Csissors.Parameters
{
    public class CancellationTokenAttributeMapper : ParameterMapper<CancellationToken>
    {
        public override Expression<Func<ITaskContext, CancellationToken>>? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo) => (context) => context.Cancellation;
    }
}