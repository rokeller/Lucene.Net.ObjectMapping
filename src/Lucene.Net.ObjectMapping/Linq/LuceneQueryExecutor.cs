using Lucene.Net.Documents;
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
    /// The base class for a generically typed query executor for LINQ queries in Lucene.Net.
    /// </summary>
    internal abstract class LuceneQueryExecutor : ExpressionVisitor
    {
        /// <summary>
        /// Executes the given Expression
        /// </summary>
        /// <param name="expression">
        /// The Expression to execute.
        /// </param>
        /// <returns>
        /// An object that represents the result of the execution.
        /// </returns>
        public abstract object Execute(Expression expression);
    }

    /// <summary>
    /// A generically typed executor for LINQ queries in Lucene.Net.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements returned with queries executed with this executor.
    /// </typeparam>
    internal sealed class LuceneQueryExecutor<T> : LuceneQueryExecutor
    {
        #region Fields

        /// <summary>
        /// The Searcher to use for the actual Lucene.Net query.
        /// </summary>
        private readonly Searcher searcher;

        /// <summary>
        /// The MappedFieldResolver to use.
        /// </summary>
        private readonly MappedFieldResolver resolver;

        #endregion

        #region C'tors

        /// <summary>
        /// Initializes a new instance of LuceneQueryParser.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to use for the search.
        /// </param>
        /// <param name="resolver">
        /// The MappedFieldResolver to use.
        /// </param>
        public LuceneQueryExecutor(Searcher searcher, MappedFieldResolver resolver)
        {
            if (null == searcher)
            {
                throw new ArgumentNullException("searcher");
            }
            else if (null == resolver)
            {
                throw new ArgumentNullException("resolver");
            }

            this.searcher = searcher;
            this.resolver = resolver;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the given Expression
        /// </summary>
        /// <param name="expression">
        /// The Expression to execute.
        /// </param>
        /// <returns>
        /// An object that represents the result of the execution.
        /// </returns>
        public override object Execute(Expression expression)
        {
            Expression result = Visit(expression);
            ConstantExpression constant = result as ConstantExpression;
            LambdaExpression lambda = result as LambdaExpression;
            Expression<Func<T>> genericLambda = result as Expression<Func<T>>;

            if (null != constant && constant.Type == typeof(LuceneQueryable<T>))
            {
                LuceneQueryable<T> queryable = (LuceneQueryable<T>)constant.Value;

                return Enumerate(queryable.Context);
            }
            else if (null != genericLambda)
            {
                Func<T> func = genericLambda.Compile();

                return func();
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
                if (node.Method.Name == "Count" && node.Arguments.Count == 1)
                {
                    Debug.Assert(node.Arguments[0].Type == typeof(LuceneQueryable<T>));
                    Debug.Assert(node.Arguments[0].NodeType == ExpressionType.Constant);

                    LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)node.Arguments[0]).Value;

                    return GetCountExpression(queryable.Context);
                }
                else if (node.Method.Name == "First")
                {
                    Debug.Assert(node.Arguments[0].Type == typeof(LuceneQueryable<T>));
                    Debug.Assert(node.Arguments[0].NodeType == ExpressionType.Constant);

                    LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)node.Arguments[0]).Value;

                    switch (node.Arguments.Count)
                    {
                        case 1:
                            return GetFirstExpression(queryable.Context);

                        case 2:
                            queryable = GetFilteredQueryable(queryable, node.Method.GetGenericArguments(), node.Arguments[1]);
                            return GetFirstExpression(queryable.Context);

                        default:
                            break;
                    }
                }
                else if (node.Method.Name == "FirstOrDefault")
                {
                    Debug.Assert(node.Arguments[0].Type == typeof(LuceneQueryable<T>));
                    Debug.Assert(node.Arguments[0].NodeType == ExpressionType.Constant);

                    LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)node.Arguments[0]).Value;

                    switch (node.Arguments.Count)
                    {
                        case 1:
                            return GetFirstOrDefaultExpression(queryable.Context);

                        case 2:
                            queryable = GetFilteredQueryable(queryable, node.Method.GetGenericArguments(), node.Arguments[1]);
                            return GetFirstOrDefaultExpression(queryable.Context);

                        default:
                            break;
                    }
                }
                else if (node.Method.Name == "Single")
                {
                    Debug.Assert(node.Arguments[0].Type == typeof(LuceneQueryable<T>));
                    Debug.Assert(node.Arguments[0].NodeType == ExpressionType.Constant);

                    LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)node.Arguments[0]).Value;

                    switch (node.Arguments.Count)
                    {
                        case 1:
                            return GetSingleExpression(queryable.Context);

                        case 2:
                            queryable = GetFilteredQueryable(queryable, node.Method.GetGenericArguments(), node.Arguments[1]);
                            return GetSingleExpression(queryable.Context);

                        default:
                            break;
                    }
                }
                else if (node.Method.Name == "SingleOrDefault")
                {
                    Debug.Assert(node.Arguments[0].Type == typeof(LuceneQueryable<T>));
                    Debug.Assert(node.Arguments[0].NodeType == ExpressionType.Constant);

                    LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)node.Arguments[0]).Value;

                    switch (node.Arguments.Count)
                    {
                        case 1:
                            return GetSingleOrDefaultExpression(queryable.Context);

                        case 2:
                            queryable = GetFilteredQueryable(queryable, node.Method.GetGenericArguments(), node.Arguments[1]);
                            return GetSingleOrDefaultExpression(queryable.Context);

                        default:
                            break;
                    }
                }
            }

            throw new NotSupportedException("The Method is not supported: " + node.Method.Name);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a filtered queryable for the given predicate.
        /// </summary>
        /// <param name="queryable">
        /// The LuceneQueryable of T to filter on.
        /// </param>
        /// <param name="typeArguments">
        /// The type arguments for the Where method call.
        /// </param>
        /// <param name="predicate">
        /// The Expression which defines the predicate.
        /// </param>
        /// <returns>
        /// A new instance of LuceneQueryable<T>.
        /// </returns>
        private LuceneQueryable<T> GetFilteredQueryable(LuceneQueryable<T> queryable, Type[] typeArguments, Expression predicate)
        {
            LuceneQueryBinder<T> binder = new LuceneQueryBinder<T>(resolver);
            Expression where = Expression.Call(typeof(Queryable),
                                               "Where",
                                               typeArguments,
                                               Expression.Constant(queryable),
                                               predicate);

            return (LuceneQueryable<T>)binder.Bind(where);
        }

        /// <summary>
        /// Enumerates the results of this query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        private IEnumerable<T> Enumerate(QueryContext context)
        {
            if (context.Skip.HasValue && context.Take.HasValue)
            {
                return EnumerateBatch(context, context.Skip.Value, context.Skip.Value + context.Take.Value);
            }

            if (context.Take.HasValue)
            {
                context.Skip = 0;
                return EnumerateBatch(context, context.Skip.Value, context.Take.Value);
            }

            return BatchedEnumerate(context);
        }

        /// <summary>
        /// Gets an Expression for a function which returns the count of the results in this query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// A Lambda expression.
        /// </returns>
        private Expression<Func<int>> GetCountExpression(QueryContext context)
        {
            return () => Count(context);
        }

        /// <summary>
        /// Gets an Expression for a function which returns the first match from a query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// A Lambda expression.
        /// </returns>
        private Expression<Func<T>> GetFirstExpression(QueryContext context)
        {
            return () => First(context);
        }

        /// <summary>
        /// Gets an Expression for a function which returns the first match or a default from a query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// A Lambda expression.
        /// </returns>
        private Expression<Func<T>> GetFirstOrDefaultExpression(QueryContext context)
        {
            return () => FirstOrDefault(context);
        }

        /// <summary>
        /// Gets an Expression for a function which returns the single match from a query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// A Lambda expression.
        /// </returns>
        private Expression<Func<T>> GetSingleExpression(QueryContext context)
        {
            return () => Single(context);
        }

        /// <summary>
        /// Gets an Expression for a function which returns the single match or the default from a query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// A Lambda expression.
        /// </returns>
        private Expression<Func<T>> GetSingleOrDefaultExpression(QueryContext context)
        {
            return () => SingleOrDefault(context);
        }

        /// <summary>
        /// Gets the Count of the results of this query.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// The number of results found with this query.
        /// </returns>
        private int Count(QueryContext context)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                context.Sort,
                1,
                false,
                false,
                false,
                false);

            searcher.Search<T>(context.Query, collector);

            if (context.Take.HasValue)
            {
                if (context.Skip.HasValue)
                {
                    int docCount = collector.TotalHits - context.Skip.Value;

                    return Math.Max(0, Math.Min(context.Take.Value, docCount));
                }
                else
                {
                    return Math.Min(context.Take.Value, collector.TotalHits);
                }
            }
            else if (context.Skip.HasValue)
            {
                // Take doesn't have a value here.
                return Math.Max(0, collector.TotalHits - context.Skip.Value);
            }

            return collector.TotalHits;
        }

        /// <summary>
        /// Gets the first matching item from the query. Throws an exception if there no items in the results.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An instance of T.
        /// </returns>
        private T First(QueryContext context)
        {
            T result = FirstOrDefault(context);

            if (null == result)
            {
                throw new InvalidOperationException("The query has no results.");
            }

            return result;
        }

        /// <summary>
        /// Gets the first matching item from the query or a default.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An instance of T, or default(T) if no items are found.
        /// </returns>
        private T FirstOrDefault(QueryContext context)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                context.Sort,
                1,
                false,
                false,
                false,
                false);

            searcher.Search<T>(context.Query, collector);

            if (collector.TotalHits < 1)
            {
                return default(T);
            }

            ScoreDoc[] scoreDocs = collector.TopDocs().ScoreDocs;
            Document doc = searcher.Doc(scoreDocs[0].Doc);

            return doc.ToObject<T>();
        }

        /// <summary>
        /// Gets the single matching item from the query. Throws an exception if there is not exactly one item in the
        /// results.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An instance of T.
        /// </returns>
        private T Single(QueryContext context)
        {
            T result = SingleOrDefault(context);

            if (null == result)
            {
                throw new InvalidOperationException("The query has no results.");
            }

            return result;
        }

        /// <summary>
        /// Gets the single matching item from the query or a default. Throws an exception if there are more than one items in the
        /// results.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An instance of T, or default(T) if no items are found.
        /// </returns>
        private T SingleOrDefault(QueryContext context)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                context.Sort,
                1,
                false,
                false,
                false,
                false);

            searcher.Search<T>(context.Query, collector);

            if (collector.TotalHits > 1)
            {
                throw new InvalidOperationException("The query has more than one result.");
            }
            else if (collector.TotalHits < 1)
            {
                return default(T);
            }

            ScoreDoc[] scoreDocs = collector.TopDocs().ScoreDocs;
            Document doc = searcher.Doc(scoreDocs[0].Doc);

            return doc.ToObject<T>();
        }

        /// <summary>
        /// Enumerates the results of this query by batching into multiple request on the Searcher.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        private IEnumerable<T> BatchedEnumerate(QueryContext context)
        {
            const int PageSize = 2000;
            int start = context.Skip.HasValue ? context.Skip.Value : 0;
            int end = start + PageSize;

            while (true)
            {
                int count = 0;

                foreach (T item in EnumerateBatch(context, start, end))
                {
                    count++;
                    yield return item;
                }

                if (0 >= count || count < PageSize)
                {
                    break;
                }

                start += PageSize;
                end += PageSize;
            }
        }

        /// <summary>
        /// Enumerates the elements of a batch with the given start (inclusive) and end (exclusive) positions.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <param name="start">
        /// The position of the first result to enumerate.
        /// </param>
        /// <param name="end">
        /// The position of the first result to stop enumeration.
        /// </param>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        private IEnumerable<T> EnumerateBatch(QueryContext context, int start, int end)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                context.Sort,
                end,
                false,
                false,
                false,
                false);

            searcher.Search<T>(context.Query, collector);

            ScoreDoc[] scoreDocs = collector.TopDocs().ScoreDocs;

            for (int i = start; i < end && i < scoreDocs.Length; i++)
            {
                Document doc = searcher.Doc(scoreDocs[i].Doc);

                yield return doc.ToObject<T>();
            }
        }

        #endregion
    }
}
