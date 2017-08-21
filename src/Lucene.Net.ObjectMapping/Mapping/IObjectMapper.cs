using Lucene.Net.Documents;
using Lucene.Net.Linq;
using Lucene.Net.Search;

namespace Lucene.Net.Mapping
{
    /// <summary>
    /// Defines the contract for a class that can map objects to documents.
    /// </summary>
    public interface IObjectMapper
    {
        /// <summary>
        /// Adds the given source object to the specified Document.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="source">
        /// The source object to add.
        /// </param>
        /// <param name="doc">
        /// The Document to add the object to.
        /// </param>
        void AddToDocument<TObject>(TObject source, Document doc);

        /// <summary>
        /// Gets the MappedFieldResolver used by this instance.
        /// </summary>
        /// <returns>
        /// A MappedFieldResolver.
        /// </returns>
        MappedFieldResolver GetMappedFieldResolver();

        /// <summary>
        /// Gets the QueryProvider used by this instance.
        /// </summary>
        /// <param name="searcher">
        /// The Searcher to use for the QueryProvider.
        /// </param>
        /// <returns>
        /// A QueryProvider.
        /// </returns>
        QueryProvider GetQueryProvider(IndexSearcher searcher);
    }
}
