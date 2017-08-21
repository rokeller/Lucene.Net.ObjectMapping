using Lucene.Net.Documents;
using Lucene.Net.Mapping;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        private readonly IndexSearcher searcher;

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
        public LuceneQueryExecutor(IndexSearcher searcher, MappedFieldResolver resolver)
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
                if (node.Method.Name == "Any")
                {
                    return GetExpressionForFilterableCall<Func<bool>>(node);
                }
                else if (node.Method.Name == "Count")
                {
                    return GetExpressionForFilterableCall<Func<int>>(node);
                }
                else if (node.Method.Name == "First" || node.Method.Name == "FirstOrDefault" ||
                         node.Method.Name == "Single" || node.Method.Name == "SingleOrDefault")
                {
                    return GetExpressionForFilterableCall<Func<T>>(node);
                }
            }

            throw new NotSupportedException("The Method is not supported: " + node.Method.Name);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a lambda expression for the method call described by the given expression.
        /// </summary>
        /// <typeparam name="TDelegate">
        /// The Type of delegate to produce the lambda expression for.
        /// </typeparam>
        /// <param name="expression">
        /// The original expression.
        /// </param>
        /// <returns>
        /// An Expression of TDelegate.
        /// </returns>
        private Expression<TDelegate> GetExpressionForFilterableCall<TDelegate>(MethodCallExpression expression)
        {
            Debug.Assert(expression.Arguments[0].Type == typeof(LuceneQueryable<T>));
            Debug.Assert(expression.Arguments[0].NodeType == ExpressionType.Constant);

            LuceneQueryable<T> queryable = (LuceneQueryable<T>)((ConstantExpression)expression.Arguments[0]).Value;
            MethodInfo method = typeof(LuceneQueryExecutor<T>).GetMethod(expression.Method.Name,
                                                                         BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(null != method, "The method info must not be null.");

            switch (expression.Arguments.Count)
            {
                case 1:
                    // We can work with the queryable the method is called on.
                    break;

                case 2:
                    // We need to apply the filter predicate from the method call on the original queryable.
                    queryable = Filter(queryable, expression.Method.GetGenericArguments(), expression.Arguments[1]);
                    break;

                default:
                    throw new NotSupportedException("The Method is not supported: " + expression.Method.Name);
            }

            // Now construct the lambda's body, as 'call this.<method>(queryable.Context)'.
            Expression body = Expression.Call(Expression.Constant(this), method, Expression.Constant(queryable.Context));
            Expression<TDelegate> lambda = Expression.Lambda<TDelegate>(body);

            return lambda;
        }

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
        /// A new instance of LuceneQueryable of T.
        /// </returns>
        private LuceneQueryable<T> Filter(LuceneQueryable<T> queryable, Type[] typeArguments, Expression predicate)
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
        /// Checks if the query from the given context yields any results.
        /// </summary>
        /// <param name="context">
        /// The QueryContext for the search.
        /// </param>
        /// <returns>
        /// True if the query has any results, false otherwise.
        /// </returns>
        private bool Any(QueryContext context)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                context.Sort,
                1,
                false,
                false,
                false,
                false);

            searcher.Search<T>(context.Query, collector);

            return collector.TotalHits > 0;
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

            ScoreDoc[] scoreDocs = collector.GetTopDocs().ScoreDocs;
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

            ScoreDoc[] scoreDocs = collector.GetTopDocs().ScoreDocs;
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

            ScoreDoc[] scoreDocs = collector.GetTopDocs().ScoreDocs;

            for (int i = start; i < end && i < scoreDocs.Length; i++)
            {
                Document doc = searcher.Doc(scoreDocs[i].Doc);

                yield return doc.ToObject<T>();
            }
        }

        #endregion
    }
}
