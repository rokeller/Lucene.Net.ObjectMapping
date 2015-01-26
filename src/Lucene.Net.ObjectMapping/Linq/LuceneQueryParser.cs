using Lucene.Net.Mapping;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// The base class for a generically typed query parser for LINQ queries in Lucene.Net.
    /// </summary>
    internal abstract class LuceneQueryParser : ExpressionVisitor
    {
        public abstract object ParseAndExecute(Expression expression);
    }

    /// <summary>
    /// A generically typed parser for LINQ queries in Lucene.Net.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements returned with queries parsed with this parser.
    /// </typeparam>
    internal sealed class LuceneQueryParser<T> : LuceneQueryParser
    {
        #region Fields

        /// <summary>
        /// The MappedFieldResolver to use.
        /// </summary>
        private readonly MappedFieldResolver resolver;

        /// <summary>
        /// The LuceneQueryable of T this parser is used with.
        /// </summary>
        private readonly LuceneQueryable<T> queryable;

        #endregion

        #region C'tors

        /// <summary>
        /// Initializes a new instance of LuceneQueryParser.
        /// </summary>
        /// <param name="queryable">
        /// The LuceneQueryable of T this parser is used with.
        /// </param>
        /// <param name="searcher">
        /// The Searcher to use for the search.
        /// </param>
        /// <param name="resolver">
        /// The MappedFieldResolver to use.
        /// </param>
        public LuceneQueryParser(LuceneQueryable<T> queryable, Searcher searcher, MappedFieldResolver resolver)
        {
            if (null == queryable)
            {
                throw new ArgumentNullException("queryable");
            }
            else if (null == searcher)
            {
                throw new ArgumentNullException("searcher");
            }
            else if (null == resolver)
            {
                throw new ArgumentNullException("resolver");
            }

            this.queryable = queryable;
            this.resolver = resolver;

            queryable.Searcher = searcher;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Parses and then executes the query from the given Expression.
        /// </summary>
        /// <param name="expression">
        /// The Expression with the query.
        /// </param>
        /// <returns>
        /// An object that represents the result of the query.
        /// </returns>
        public override object ParseAndExecute(Expression expression)
        {
            Expression result = Visit(expression);
            ConstantExpression constant = result as ConstantExpression;
            LambdaExpression lambda = result as LambdaExpression;

            if (null != constant && constant.Type == typeof(LuceneQueryable<T>))
            {
                Debug.Assert(queryable == constant.Value, "The current queryable must the equal to the constant.");
                return queryable.Enumerate();
            }
            else if (null != lambda)
            {
                return lambda.Compile().DynamicInvoke();
            }

            throw new NotSupportedException("The expression is not supported: " + expression);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Visits the children of the MethodCallExpression.
        /// </summary>
        /// <param name="node">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                if (node.Method.Name == "Count")
                {
                    Visit(node.Arguments[0]);

                    return queryable.CountExpression;
                }
                else if (node.Method.Name == "Skip" && node.Arguments[1].Type == typeof(int))
                {
                    Visit(node.Arguments[0]);
                    queryable.Context.Skip = node.Arguments[1].GetValue<int>();

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "Take" && node.Arguments[1].Type == typeof(int))
                {
                    Visit(node.Arguments[0]);
                    queryable.Context.Take = node.Arguments[1].GetValue<int>();

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "Where")
                {
                    Visit(node.Arguments[0]);
                    queryable.Context.Query = resolver.GetQuery(node.Arguments[1]);

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "OrderBy" && node.Arguments.Count == 2)
                {
                    Visit(node.Arguments[0]);
                    SortField newSortField = resolver.GetSortField(node.Arguments[1], false);

                    queryable.Context.Sort = new Sort(newSortField);

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "OrderByDescending" && node.Arguments.Count == 2)
                {
                    Visit(node.Arguments[0]);
                    SortField newSortField = resolver.GetSortField(node.Arguments[1], true);

                    queryable.Context.Sort = new Sort(newSortField);

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "ThenBy" && node.Arguments.Count == 2)
                {
                    Visit(node.Arguments[0]);
                    SortField newSortField = resolver.GetSortField(node.Arguments[1], false);

                    List<SortField> sortFields = new List<SortField>(queryable.Context.Sort.GetSort());
                    sortFields.Add(newSortField);
                    queryable.Context.Sort.SetSort(sortFields.ToArray());

                    return Expression.Constant(queryable);
                }
                else if (node.Method.Name == "ThenByDescending" && node.Arguments.Count == 2)
                {
                    Visit(node.Arguments[0]);
                    SortField newSortField = resolver.GetSortField(node.Arguments[1], true);

                    List<SortField> sortFields = new List<SortField>(queryable.Context.Sort.GetSort());
                    sortFields.Add(newSortField);
                    queryable.Context.Sort.SetSort(sortFields.ToArray());

                    return Expression.Constant(queryable);
                }
            }

            throw new NotSupportedException("The Method is not supported: " + node.Method.Name);
        }

        /// <summary>
        /// Visits the ConstantExpression.
        /// </summary>
        /// <param name="node">
        /// The expression to visit.
        /// </param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            IQueryable queryable = node.Value as IQueryable;

            if (null != queryable)
            {
                Type type = queryable.GetType();

                if (type.IsGenericType)
                {
                    Type genericType = type.GetGenericTypeDefinition();

                    if (typeof(LuceneQueryable<>) == genericType ||
                        genericType.IsSubclassOf(typeof(LuceneQueryable<>)))
                    {
                        return node;
                    }
                }
            }

            return base.VisitConstant(node);
        }

        #endregion
    }
}
