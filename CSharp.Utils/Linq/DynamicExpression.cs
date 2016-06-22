using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CSharp.Utils.Linq
{
    public static class DynamicExpression
    {
        #region Methods

        public static Type CreateClass(params DynamicProperty[] properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(IEnumerable<DynamicProperty> properties)
        {
            return ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Expression Parse(Type resultType, string expression, params object[] values)
        {
            var parser = new ExpressionParser(null, expression, values);
            return parser.Parse(resultType);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, 
            params object[] values)
        {
            return ParseLambda(new[] {Expression.Parameter(itType, string.Empty)}, resultType, expression, 
                values);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, 
            string expression, 
            params object[] values)
        {
            var parser = new ExpressionParser(parameters, expression, values);
            return Expression.Lambda(parser.Parse(resultType), parameters);
        }

        public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, 
            params object[] values)
        {
            return (Expression<Func<T, S>>) ParseLambda(typeof (T), typeof (S), expression, values);
        }

        #endregion Methods
    }
}
