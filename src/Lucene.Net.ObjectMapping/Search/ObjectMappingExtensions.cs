using Lucene.Net.Documents;
using System;

namespace Lucene.Net.Search
{
    /// <summary>
    /// Extension class to help searching for Documents which are mapped from objects.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        #region Plain Query, no Sort

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="type">
        /// The type of the object to search documents for.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search(this Searcher searcher, Type type, Query query, int numResults)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter(type), numResults);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="type">
        /// The type of the object to search documents for.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search(this Searcher searcher, Type type, DocumentObjectTypeKind kind, Query query, int numResults)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter(type, kind), numResults);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to search documents for.
        /// </typeparam>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search<TObject>(this Searcher searcher, Query query, int numResults)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(), numResults);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to search documents for.
        /// </typeparam>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search<TObject>(this Searcher searcher, DocumentObjectTypeKind kind, Query query, int numResults)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(kind), numResults);
        }

        #endregion

        #region With Sort

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="type">
        /// The type of the object to search documents for.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <param name="sort">
        /// A Sort object that defines how to sort the results.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search(this Searcher searcher, Type type, Query query, int numResults, Sort sort)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter(type), numResults, sort);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="type">
        /// The type of the object to search documents for.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <param name="sort">
        /// A Sort object that defines how to sort the results.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search(this Searcher searcher, Type type, DocumentObjectTypeKind kind, Query query, int numResults, Sort sort)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter(type, kind), numResults, sort);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to search documents for.
        /// </typeparam>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <param name="sort">
        /// A Sort object that defines how to sort the results.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search<TObject>(this Searcher searcher, Query query, int numResults, Sort sort)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(), numResults, sort);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to search documents for.
        /// </typeparam>
        /// <param name="searcher">
        /// The Searcher to search on.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="query">
        /// The Query which selects the documents.
        /// </param>
        /// <param name="numResults">
        /// The number of results to return.
        /// </param>
        /// <param name="sort">
        /// A Sort object that defines how to sort the results.
        /// </param>
        /// <returns>
        /// An instance of TopDocs.
        /// </returns>
        public static TopDocs Search<TObject>(this Searcher searcher, DocumentObjectTypeKind kind, Query query, int numResults, Sort sort)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(kind), numResults, sort);
        }

        #endregion
    }
}
