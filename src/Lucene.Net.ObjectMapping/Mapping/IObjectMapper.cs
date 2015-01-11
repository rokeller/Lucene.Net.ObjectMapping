using Lucene.Net.Documents;

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
    }
}
