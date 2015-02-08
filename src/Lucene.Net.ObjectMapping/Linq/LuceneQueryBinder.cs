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
    internal abstract class LuceneQueryBinder : ExpressionVisitor
    {
        /// <summary>
        /// Binds the given Expression and returns a new IQueryable.
        /// </summary>
        /// <param name="expression">
        /// The Expression to bind.
        /// </param>
        /// <returns>
        /// An instance of IQueryable.
        /// </returns>
        public abstract IQueryable Bind(Expression expression);
    }

    /// <summary>
    /// A generically typed parser for LINQ queries in Lucene.Net.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements returned with queries parsed with this parser.
    /// </typeparam>
    internal sealed class LuceneQueryBinder<T> : LuceneQueryBinder
    {
        #region Fields

        /// <summary>
        /// The MappedFieldResolver to use.
        /// </summary>
        private readonly MappedFieldResolver resolver;

        /// <summary>
        /// The LuceneQueryable of T this parser is used with.
        /// </summary>
        private LuceneQueryable<T> queryable;

        #endregion

        #region C'tors

        /// <summary>
        /// Initializes a new instance of LuceneQueryParser.
        /// </summary>
        /// <param name="resolver">
        /// The MappedFieldResolver to use.
        /// </param>
        public LuceneQueryBinder(MappedFieldResolver resolver)
        {
            if (null == resolver)
            {
                throw new ArgumentNullException("resolver");
            }

            this.resolver = resolver;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Binds the given Expression and returns a LuceneQueryable of T.
        /// </summary>
        /// <param name="expression">
        /// The Expression to bind.
        /// </param>
        /// <returns>
        /// An instance of LuceneQueryable of T.
        /// </returns>
        public override IQueryable Bind(Expression expression)
        {
            Visit(expression);

            return queryable;
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
                if (node.Method.Name == "Skip" && node.Arguments[1].Type == typeof(int))
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
                else if (node.Method.Name == "Where" && node.Arguments[1].Type == typeof(Expression<Func<T, bool>>))
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
            LuceneQueryable<T> queryable = node.Value as LuceneQueryable<T>;

            if (null != queryable)
            {
                Debug.Assert(null == this.queryable, "The queryable must not have been set yet.");
                this.queryable = new LuceneQueryable<T>(queryable.QueryProvider, QueryContext.Clone(queryable.Context));

                return node;
            }

            return base.VisitConstant(node);
        }

        #endregion
    }
}
