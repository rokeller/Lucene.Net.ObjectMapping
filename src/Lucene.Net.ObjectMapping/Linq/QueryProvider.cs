using Lucene.Net.Mapping;
using Lucene.Net.Search;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// Base class for IQueryProvider objects supporting the LuceneQueryable.
    /// </summary>
    public abstract class QueryProvider : IQueryProvider
    {
        #region C'tors

        /// <summary>
        /// Initializes a new QueryProvider for the given Searcher.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to use.
        /// </param>
        protected QueryProvider(IndexSearcher searcher)
        {
            if (null == searcher)
            {
                throw new ArgumentNullException("searcher");
            }

            Searcher = searcher;
        }

        #endregion

        #region Public Interface

        #region IQueryProvider Implementation

        /// <summary>
        /// Constructs an IQueryable&lt;TElement&gt; object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements of the IQueryable&lt;TElement&gt; that is returned.
        /// </typeparam>
        /// <param name="expression">
        /// The Expression to create a query for.
        /// </param>
        /// <returns>
        /// An IQueryable&lt;TElement&gt; that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)Bind(expression);
        }

        /// <summary>
        /// Constructs an IQueryable object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">
        /// An expression tree that represents a LINQ query.
        /// </param>
        /// <returns>
        /// An IQueryable that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)Bind(expression);
        }

        /// <summary>
        /// Executes the strongly-typed query represented by a specified expression tree.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the value that results from executing the query.
        /// </typeparam>
        /// <param name="expression">
        /// An expression tree that represents a LINQ query.
        /// </param>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)ExecuteQueryWorker(expression);
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">
        /// An expression tree that represents a LINQ query.
        /// </param>
        /// <returns>
        /// The value that results from executing the specified query.
        /// </returns>
        public object Execute(Expression expression)
        {
            return ExecuteQueryWorker(expression);
        }

        #endregion

        /// <summary>
        /// Gets the FieldNameResolver to use with this instance.
        /// </summary>
        public abstract MappedFieldResolver FieldNameResolver { get; }

        #endregion

        #region Internal Interface

        /// <summary>
        /// Gets or sets the IQueryable managed with this instance.
        /// </summary>
        internal IQueryable Queryable { get; set; }

        /// <summary>
        /// Gets or sets the Searcher used with this instance.
        /// </summary>
        internal IndexSearcher Searcher { get; private set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Binds the given Expression and creates a new query for it.
        /// </summary>
        /// <param name="expression">
        /// The Expression to bind.
        /// </param>
        /// <returns>
        /// An instance of IQueryable.
        /// </returns>
        private IQueryable Bind(Expression expression)
        {
            Type enumerableType = Queryable.GetType().GetEnumerableElementType();
            Type binderType = typeof(LuceneQueryBinder<>).MakeGenericType(enumerableType);

            LuceneQueryBinder binder = (LuceneQueryBinder)Activator.CreateInstance(
                binderType,
                FieldNameResolver);

            return binder.Bind(expression);
        }

        /// <summary>
        /// Executes the given Expression and returns the result.
        /// </summary>
        /// <param name="expression">
        /// The Expression to execute.
        /// </param>
        /// <returns>
        /// An object that contains the result of the expression.
        /// </returns>
        private object ExecuteQueryWorker(Expression expression)
        {
            Type enumerableType = Queryable.GetType().GetEnumerableElementType();
            Type executorType = typeof(LuceneQueryExecutor<>).MakeGenericType(enumerableType);

            LuceneQueryExecutor executor = (LuceneQueryExecutor)Activator.CreateInstance(executorType,
                Searcher,
                FieldNameResolver);

            return executor.Execute(expression);
        }

        #endregion
    }
}
