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
        internal LuceneQueryable(QueryProvider provider) : this(provider, new QueryContext()) { }

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
            return String.Format("LuceneQueryable<{0}>({1:x8}): {2}", typeof(T), this.GetHashCode(), Context);
        }

        #endregion

        #region Internal Interface

        /// <summary>
        /// Gets or sets the QueryContext for this instance.
        /// </summary>
        internal QueryContext Context { get; private set; }

        #endregion
    }
}
