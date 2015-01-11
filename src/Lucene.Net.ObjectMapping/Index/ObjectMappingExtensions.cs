using Lucene.Net.Documents;
using Lucene.Net.Search;
using System;

namespace Lucene.Net.Index
{
    /// <summary>
    /// ObjectMapping related Extensions for the Lucene.Net.Index namespace.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        /// <summary>
        /// Deletes the documents for objects of the given type matching the given selection.
        /// </summary>
        /// <param name="writer">
        /// The IndexWriter to delete the documents from.
        /// </param>
        /// <param name="type">
        /// The type of the object to delete documents for.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the documents to delete.
        /// </param>
        public static void DeleteDocuments(this IndexWriter writer, Type type, Query selection)
        {
            Query deleteQuery = new FilteredQuery(selection, ObjectMapping.GetTypeFilter(type));

            writer.DeleteDocuments(deleteQuery);
        }

        /// <summary>
        /// Deletes the documents for objects of the given type matching the given selection.
        /// </summary>
        /// <param name="writer">
        /// The IndexWriter to delete the documents from.
        /// </param>
        /// <param name="type">
        /// The type of the object to delete documents for.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the documents to delete.
        /// </param>
        public static void DeleteDocuments(this IndexWriter writer, Type type, DocumentObjectTypeKind kind, Query selection)
        {
            Query deleteQuery = new FilteredQuery(selection, ObjectMapping.GetTypeFilter(type, kind));

            writer.DeleteDocuments(deleteQuery);
        }

        /// <summary>
        /// Deletes the documents for objects of the given type matching the given selection.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to delete documents for.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to delete the documents from.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the documents to delete.
        /// </param>
        public static void DeleteDocuments<TObject>(this IndexWriter writer, Query selection)
        {
            Query deleteQuery = new FilteredQuery(selection, ObjectMapping.GetTypeFilter<TObject>());

            writer.DeleteDocuments(deleteQuery);
        }

        /// <summary>
        /// Deletes the documents for objects of the given type matching the given selection.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of the object to delete documents for.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to delete the documents from.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the search to.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the documents to delete.
        /// </param>
        public static void DeleteDocuments<TObject>(this IndexWriter writer, DocumentObjectTypeKind kind, Query selection)
        {
            Query deleteQuery = new FilteredQuery(selection, ObjectMapping.GetTypeFilter<TObject>(kind));

            writer.DeleteDocuments(deleteQuery);
        }
    }
}
