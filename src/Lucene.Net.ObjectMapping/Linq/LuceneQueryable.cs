using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// Implements the IOrderedQueryable of T for Lucene.Net based queries.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the content of the data source.
    /// </typeparam>
    public class LuceneQueryable<T> : IOrderedQueryable<T>
    {
        #region C'tors

        /// <summary>
        /// Initializes a new instance of LuceneQueryable.
        /// </summary>
        /// <param name="provider">
        /// The QueryProvider to use.
        /// </param>
        /// <param name="context">
        /// The QueryContext to use.
        /// </param>
        internal LuceneQueryable(QueryProvider provider, QueryContext context)
        {
            if (null == provider)
            {
                throw new ArgumentNullException("provider");
            }
            else if (null == context)
            {
                throw new ArgumentNullException("context");
            }

            QueryProvider = provider;
            QueryProvider.Queryable = this;
            Context = context;
            Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a new instance of LuceneQueryable.
        /// </summary>
        /// <param name="provider">
        /// The QueryProvider to use.
        /// </param>
        /// <param name="context">
        /// The QueryContext to use.
        /// </param>
        /// <param name="expression">
        /// The Expression to use.
        /// </param>
        internal LuceneQueryable(QueryProvider provider, QueryContext context, Expression expression)
        {
            if (null == provider)
            {
                throw new ArgumentNullException("provider");
            }
            else if (null == context)
            {
                throw new ArgumentNullException("context");
            }
            else if (null == expression)
            {
                throw new ArgumentNullException("expression");
            }

            QueryProvider = provider;
            QueryProvider.Queryable = this;
            Context = context;
            Expression = expression;
        }

        #endregion

        #region Public Interface

        #region IOrderedQueryable<T> Interface

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A IEnumerator&lt;T&gt; that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return QueryProvider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An IEnumerator object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)QueryProvider.Execute(Expression)).GetEnumerator();
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance
        /// is executed.
        /// </summary>
        public Type ElementType { get { return typeof(T); } }

        /// <summary>
        /// Gets or sets the expression tree that is associated with this instance instance.
        /// </summary>
        public Expression Expression { get; private set; }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public IQueryProvider Provider { get { return QueryProvider; } }

        #endregion

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public QueryProvider QueryProvider { get; private set; }

        /// <summary>
        /// Returns a string repesentation of this instance.
        /// </summary>
        /// <returns>
        /// A string which represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("LuceneQueryable<{0}>: {1}", typeof(T), Context);
        }

        #endregion

        #region Internal Interface

        /// <summary>
        /// Gets or sets the QueryContext for this instance.
        /// </summary>
        internal QueryContext Context { get; private set; }

        /// <summary>
        /// Gets or sets the Searcher for this instance.
        /// </summary>
        internal Searcher Searcher { get; set; }

        /// <summary>
        /// Enumerates the results of this query.
        /// </summary>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        internal IEnumerable<T> Enumerate()
        {
            if (Context.Skip.HasValue && Context.Take.HasValue)
            {
                return EnumerateBatch(Context.Skip.Value, Context.Skip.Value + Context.Take.Value);
            }

            if (Context.Take.HasValue)
            {
                Context.Skip = 0;
                return EnumerateBatch(Context.Skip.Value, Context.Take.Value);
            }

            return BatchedEnumerate();
        }

        /// <summary>
        /// Gets the Count of the results of this query.
        /// </summary>
        /// <returns>
        /// The number of results found with this query.
        /// </returns>
        internal int Count()
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                Context.Sort,
                1,
                false,
                false,
                false,
                false);

            Searcher.Search<T>(Context.Query, collector);

            if (Context.Take.HasValue)
            {
                if (Context.Skip.HasValue)
                {
                    int docCount = collector.TotalHits - Context.Skip.Value;

                    return Math.Max(0, Math.Min(Context.Take.Value, docCount));
                }
                else
                {
                    return Math.Min(Context.Take.Value, collector.TotalHits);
                }
            }
            else if (Context.Skip.HasValue)
            {
                // Take doesn't have a value here.
                return Math.Max(0, collector.TotalHits - Context.Skip.Value);
            }

            return collector.TotalHits;
        }

        /// <summary>
        /// Gets an Expression for a function which returns the count of the results in this query.
        /// </summary>
        internal Expression<Func<int>> CountExpression
        {
            get { return () => Count(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Enumerates the results of this query by batching into multiple request on the Searcher.
        /// </summary>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        private IEnumerable<T> BatchedEnumerate()
        {
            const int PageSize = 2000;
            int start = Context.Skip.HasValue ? Context.Skip.Value : 0;
            int end = start + PageSize;

            while (true)
            {
                int count = 0;

                foreach (T item in EnumerateBatch(start, end))
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
        /// <param name="start">
        /// The position of the first result to enumerate.
        /// </param>
        /// <param name="end">
        /// The position of the first result to stop enumeration.
        /// </param>
        /// <returns>
        /// An IEnumerable of T which yields the results of this query.
        /// </returns>
        private IEnumerable<T> EnumerateBatch(int start, int end)
        {
            TopFieldCollector collector = TopFieldCollector.Create(
                Context.Sort,
                end,
                false,
                false,
                false,
                false);

            Searcher.Search<T>(Context.Query, collector);

            ScoreDoc[] scoreDocs = collector.TopDocs().ScoreDocs;

            for (int i = start; i < end && i < scoreDocs.Length; i++)
            {
                Document doc = Searcher.Doc(scoreDocs[i].Doc);

                yield return doc.ToObject<T>();
            }
        }

        #endregion
    }
}
