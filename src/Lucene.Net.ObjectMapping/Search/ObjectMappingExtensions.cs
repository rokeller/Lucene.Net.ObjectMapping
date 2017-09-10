using Lucene.Net.Documents;
using Lucene.Net.Linq;
using Lucene.Net.Mapping;
using System;
using System.Linq;

namespace Lucene.Net.Search
{
    /// <summary>
    /// Extension class to help searching for Documents which are mapped from objects.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        /// <summary>
        /// Creates an IQueryable to find documents of type TElement.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of objects to return with the query.
        /// </typeparam>
        /// <param name="searcher">
        /// The Searcher to user.
        /// </param>
        /// <returns>
        /// An IQueryable of TElement.
        /// </returns>
        public static IQueryable<TElement> AsQueryable<TElement>(this IndexSearcher searcher)
        {
            if (null == searcher)
            {
                throw new ArgumentNullException("searcher");
            }

            MappingSettings settings = MappingSettings.Default;

            return new LuceneQueryable<TElement>(settings.ObjectMapper.GetQueryProvider(searcher));
        }

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
        public static TopDocs Search(this IndexSearcher searcher, Type type, Query query, int numResults)
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
        public static TopDocs Search(this IndexSearcher searcher, Type type, DocumentObjectTypeKind kind, Query query, int numResults)
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
        public static TopDocs Search<TObject>(this IndexSearcher searcher, Query query, int numResults)
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
        public static TopDocs Search<TObject>(this IndexSearcher searcher, DocumentObjectTypeKind kind, Query query, int numResults)
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
        public static TopDocs Search(this IndexSearcher searcher, Type type, Query query, int numResults, Sort sort)
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
        public static TopDocs Search(this IndexSearcher searcher, Type type, DocumentObjectTypeKind kind, Query query, int numResults, Sort sort)
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
        public static TopDocs Search<TObject>(this IndexSearcher searcher, Query query, int numResults, Sort sort)
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
        public static TopDocs Search<TObject>(this IndexSearcher searcher, DocumentObjectTypeKind kind, Query query, int numResults, Sort sort)
        {
            return searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(kind), numResults, sort);
        }

        #endregion

        #region Search with Collector

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query and Collector.
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
        /// <param name="results">
        /// The Collector to use to gather results.
        /// </param>
        public static void Search(this IndexSearcher searcher, Type type, Query query, ICollector results)
        {
            searcher.Search(query, ObjectMapping.GetTypeFilter(type), results);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query and Collector.
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
        /// <param name="results">
        /// The Collector to use to gather results.
        /// </param>
        public static void Search(this IndexSearcher searcher, Type type, DocumentObjectTypeKind kind, Query query, ICollector results)
        {
            searcher.Search(query, ObjectMapping.GetTypeFilter(type, kind), results);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query and Collector.
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
        /// <param name="results">
        /// The Collector to use to gather results.
        /// </param>
        public static void Search<TObject>(this IndexSearcher searcher, Query query, ICollector results)
        {
            searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(), results);
        }

        /// <summary>
        /// Searches for documents mapped from the given type using the specified query and Collector.
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
        /// <param name="results">
        /// The Collector to use to gather results.
        /// </param>
        public static void Search<TObject>(this IndexSearcher searcher, DocumentObjectTypeKind kind, Query query, ICollector results)
        {
            searcher.Search(query, ObjectMapping.GetTypeFilter<TObject>(kind), results);
        }

        #endregion
    }
}
