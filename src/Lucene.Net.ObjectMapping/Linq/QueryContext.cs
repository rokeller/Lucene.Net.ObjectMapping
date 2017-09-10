using Lucene.Net.Search;
using System;
using System.Diagnostics;

namespace Lucene.Net.Linq
{
    /// <summary>
    /// The context of a query in Lucene.Net
    /// </summary>
    internal sealed class QueryContext
    {
        /// <summary>
        /// Gets or sets the Query to use.
        /// </summary>
        public Query Query { get; set; }

        /// <summary>
        /// Gets or sets the Sort to use.
        /// </summary>
        public Sort Sort { get; set; }

        /// <summary>
        /// Gets or sets the number of results to skip, or null if no results should be skipped.
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Gets or sets the number of results to take, or null if all results should be taken.
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// Initializes a new instance of QueryContext.
        /// </summary>
        public QueryContext()
        {
            InitQueryParams();
        }

        /// <summary>
        /// Gets a string representation of this instance.
        /// </summary>
        /// <returns>
        /// A string which represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Query=[{0}], Sort=[{1}], Skip={2}, Take={3}", Query, Sort, Skip, Take);
        }

        /// <summary>
        /// Clones the given QueryContext.
        /// </summary>
        /// <param name="context">
        /// The QueryContext to clone.
        /// </param>
        /// <returns>
        /// A new instance of QueryContext, which builds on the data of the specified context.
        /// </returns>
        internal static QueryContext Clone(QueryContext context)
        {
            Debug.Assert(null != context, "The context must not be null.");

            QueryContext result = new QueryContext();

            if (!(context.Query is MatchAllDocsQuery))
            {
                result.Query = context.Query;
            }

            SortField[] fields = context.Sort.GetSort();
            if (fields.Length != 1 || fields[0].Type != SortFieldType.SCORE)
            {
                result.Sort = new Sort(fields);
            }

            if (result.Take != context.Take)
            {
                result.Take = context.Take;
            }

            if (result.Skip != context.Skip)
            {
                result.Skip = context.Skip;
            }

            return result;
        }

        /// <summary>
        /// Initializes the query parameters for this instance.
        /// </summary>
        private void InitQueryParams()
        {
            Query = new MatchAllDocsQuery();
            Sort = new Sort();
        }
    }
}
