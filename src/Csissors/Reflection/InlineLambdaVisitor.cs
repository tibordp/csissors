using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Csissors.Reflection
{
    internal class InlineLambdaVisitor : ExpressionVisitor
    {
        private readonly Expression[] _replacements;
        private ReadOnlyCollection<ParameterExpression>? _parameters;

        private InlineLambdaVisitor(params Expression[] replacements)
        {
            _replacements = replacements;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (_parameters == null)
            {
                if (node.Parameters.Count != _replacements.Length)
                {
                    throw new ArgumentException("Parameter count mismatch");
                }
                _parameters = node.Parameters;
                return Visit(node.Body);
            }
            else
            {
                return base.VisitLambda(node);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parameters != null)
            {
                var parameterIndex = _parameters.IndexOf(node);
                if (parameterIndex != -1)
                {
                    return _replacements[parameterIndex];
                }
            }
            return base.VisitParameter(node);
        }

        public static Expression InlineLambda<T>(Expression<T> node, params Expression[] replacements)
        {
            return new InlineLambdaVisitor(replacements).VisitLambda(node);
        }
    }
}