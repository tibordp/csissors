using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Csissors.Parameters
{
    public class ServiceAttributeMapper : IParameterMapper
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceAttributeMapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Expression? MapParameter(MethodInfo taskMethodInfo, ParameterInfo parameterInfo, ParameterExpression contextParameter)
        {
            var service = _serviceProvider.GetService(parameterInfo.ParameterType);
            return service != null ? Expression.Constant(service) : default;
        }
    }
}