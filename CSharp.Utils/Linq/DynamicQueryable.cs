using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Linq
{
    public static class DynamicQueryable
    {
        #region Methods

        public static bool Any(this IQueryable source)
        {
            Guard.ArgumentNotNull(source, "source");
            return (bool) source.Provider.Execute(
                Expression.Call(
                    typeof (Queryable), "Any", 
                    new[] {source.ElementType}, source.Expression));
        }

        public static int Count(this IQueryable source)
        {
            Guard.ArgumentNotNull(source, "source");
            return (int) source.Provider.Execute(
                Expression.Call(
                    typeof (Queryable), "Count", 
                    new[] {source.ElementType}, source.Expression));
        }

        public static IQueryable GroupBy(this IQueryable source, string keySelector, 
            string elementSelector, 
            params object[] values)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(keySelector, "keySelector");
            Guard.ArgumentNotNull(elementSelector, "elementSelector");
            LambdaExpression keyLambda = DynamicExpression.ParseLambda(source.ElementType, null, 
                keySelector, values);
            LambdaExpression elementLambda = DynamicExpression.ParseLambda(source.ElementType, null, 
                elementSelector, 
                values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof (Queryable), "GroupBy", 
                    new[] {source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type}, 
                    source.Expression, Expression.Quote(keyLambda), Expression.Quote(elementLambda)));
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, 
            params object[] values)
        {
            return (IQueryable<T>) OrderBy((IQueryable) source, ordering, values);
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, 
            params object[] values)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(ordering, "ordering");
            var parameters = new[]
            {
                Expression.Parameter(source.ElementType, string.Empty)
            };
            var parser = new ExpressionParser(parameters, ordering, values);
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings)
            {
                queryExpr = Expression.Call(
                    typeof (Queryable), o.Ascending ? methodAsc : methodDesc, 
                    new[] {source.ElementType, o.Selector.Type}, 
                    queryExpr, Expression.Quote(Expression.Lambda(o.Selector, parameters)));
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }

            return source.Provider.CreateQuery(queryExpr);
        }

        public static IQueryable Select(this IQueryable source, string selector, 
            params object[] values)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(selector, "selector");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, null, 
                selector, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof (Queryable), "Select", 
                    new[] {source.ElementType, lambda.Body.Type}, 
                    source.Expression, Expression.Quote(lambda)));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            Guard.ArgumentNotNull(source, "source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof (Queryable), "Skip", 
                    new[] {source.ElementType}, 
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable Take(this IQueryable source, int count)
        {
            Guard.ArgumentNotNull(source, "source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof (Queryable), "Take", 
                    new[] {source.ElementType}, 
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, 
            params object[] values)
        {
            return (IQueryable<T>) Where((IQueryable) source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, 
            params object[] values)
        {
            Guard.ArgumentNotNull(source, "source");
            Guard.ArgumentNotNull(predicate, "predicate");
            LambdaExpression lambda = DynamicExpression.ParseLambda(source.ElementType, 
                typeof (bool), predicate, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof (Queryable), "Where", 
                    new[] {source.ElementType}, 
                    source.Expression, Expression.Quote(lambda)));
        }

        #endregion Methods
    }
}
