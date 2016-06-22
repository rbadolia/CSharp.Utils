using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Linq
{
    public static class DynamicQueryableExtras
    {
        #region Methods

        public static IQueryable Distinct(this IQueryable q)
        {
            MethodCallExpression call = Expression.Call(typeof (Queryable), "Distinct", 
                new[] {q.ElementType}, 
                q.Expression);
            return q.Provider.CreateQuery(call);
        }

        public static IQueryable Join(this IQueryable outer, IEnumerable inner, string outerSelector, 
            string innerSelector, string resultsSelector, params object[] values)
        {
            Guard.ArgumentNotNull(inner, "inner");
            Guard.ArgumentNotNull(outerSelector, "outerSelector");
            Guard.ArgumentNotNull(innerSelector, "innerSelector");
            Guard.ArgumentNotNull(resultsSelector, "resultsSelector");
            LambdaExpression outerSelectorLambda = DynamicExpression.ParseLambda(outer.ElementType, 
                null, outerSelector, 
                values);
            LambdaExpression innerSelectorLambda =
                DynamicExpression.ParseLambda(inner.AsQueryable().ElementType, null, 
                    innerSelector, values);
            var parameters = new[]
            {
                Expression.Parameter(outer.ElementType, "outer"), 
                Expression.Parameter(inner.AsQueryable().ElementType, "inner")
            };
            LambdaExpression resultsSelectorLambda = DynamicExpression.ParseLambda(parameters, null, 
                resultsSelector, 
                values);
            return outer.Provider.CreateQuery(
                Expression.Call(typeof (Queryable), "Join", 
                    new[]
                    {
                        outer.ElementType, 
                        inner.AsQueryable().ElementType, 
                        outerSelectorLambda.Body.Type, 
                        resultsSelectorLambda.Body.Type
                    }, outer.Expression, 
                    inner.AsQueryable().Expression, 
                    Expression.Quote(outerSelectorLambda), 
                    Expression.Quote(innerSelectorLambda), 
                    Expression.Quote(resultsSelectorLambda)));
        }

        public static IQueryable<T> Join<T>(this IQueryable<T> outer, IEnumerable<T> inner, 
            string outerSelector, 
            string innerSelector, string resultsSelector, params object[] values)
        {
            return
                (IQueryable<T>)
                    Join(outer, (IEnumerable) inner, outerSelector, innerSelector, resultsSelector, 
                        values);
        }

        #endregion Methods
    }
}
