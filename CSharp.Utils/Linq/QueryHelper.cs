using System.Linq;
using System.Reflection;
using System.Text;

namespace CSharp.Utils.Linq
{
    public static class QueryHelper
    {
        #region Methods

        public static IQueryable GroupBy(IQueryable enumerable, string groupBy)
        {
            return GroupBy(enumerable, groupBy, null, null);
        }

        public static IQueryable GroupBy(IQueryable enumerable, string groupBy, string havingClause)
        {
            return GroupBy(enumerable, groupBy, havingClause, null);
        }

        public static IQueryable GroupBy(IQueryable enumerable, string groupBy, string havingClause, 
            string select, 
            string orderBy)
        {
            IQueryable result = enumerable;
            if (!string.IsNullOrEmpty(groupBy))
            {
                result = enumerable.GroupBy(string.Format("new({0})", groupBy), "it");
            }

            if (!string.IsNullOrEmpty(havingClause))
            {
                result = result.Where(havingClause);
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                result.OrderBy(orderBy);
            }

            if (string.IsNullOrEmpty(select) || select == "*")
            {
                PropertyInfo[] properties = enumerable.ElementType.GetProperties();
                var sb = new StringBuilder();
                if (properties.Length > 0)
                {
                    foreach (PropertyInfo p in properties)
                    {
                        sb.Append(p.Name);
                        sb.Append(',');
                    }

                    sb.Length--;
                    select = sb.ToString();
                }
            }

            if (!string.IsNullOrEmpty(select))
            {
                result = SelectFields(result, select);
            }

            return result;
        }

        public static IQueryable GroupBy(IQueryable enumerable, string groupBy, string havingClause, 
            string select)
        {
            IQueryable result = enumerable;
            if (!string.IsNullOrEmpty(groupBy))
            {
                result = enumerable.GroupBy(string.Format("new({0})", groupBy), "it");
            }

            if (!string.IsNullOrEmpty(havingClause))
            {
                result = result.Where(havingClause);
            }

            if (!string.IsNullOrEmpty(select))
            {
                result = SelectFields(result, select);
            }

            return result;
        }

        public static IQueryable Query(this IQueryable queryable, string select, string where, 
            string groupBy, 
            string having, string orderBy)
        {
            IQueryable result = queryable;
            if (!string.IsNullOrEmpty(where))
            {
                result = queryable.Where(where);
            }

            return GroupBy(result, groupBy, having, select, orderBy);
        }

        public static IQueryable SelectFields(this IQueryable queryable, string fields)
        {
            return queryable.Select(string.Format("new({0})", fields));
        }

        #endregion Methods
    }
}
