using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Mapping;
using Lucene.Net.Search;
using System;
using System.Linq.Expressions;

namespace Lucene.Net.Index
{
    /// <summary>
    /// ObjectMapping related Extensions for the Lucene.Net.Index namespace.
    /// </summary>
    public static class ObjectMappingExtensions
    {
        #region Add

        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        public static void Add<T>(this IndexWriter writer, T obj)
        {
            Add<T>(writer, obj, MappingSettings.Default);
        }

        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        public static void Add<T>(this IndexWriter writer, T obj, MappingSettings settings)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }

            writer.AddDocument(obj.ToDocument<T>(settings));
        }

        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Add<T>(this IndexWriter writer, T obj, Analyzer analyzer)
        {
            Add<T>(writer, obj, MappingSettings.Default, analyzer);
        }

        /// <summary>
        /// Adds the specified object to the given IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to add.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to use.
        /// </param>
        /// <param name="obj">
        /// The object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Add<T>(this IndexWriter writer, T obj, MappingSettings settings, Analyzer analyzer)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == analyzer)
            {
                throw new ArgumentNullException("analyzer");
            }

            writer.AddDocument(obj.ToDocument<T>(settings), analyzer);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, Expression<Func<T, bool>> predicate)
        {
            Update<T>(writer, obj, MappingSettings.Default, predicate);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, MappingSettings settings, Expression<Func<T, bool>> predicate)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == predicate)
            {
                throw new ArgumentNullException("predicate");
            }

            MappedFieldResolver resolver = settings.ObjectMapper.GetMappedFieldResolver();
            Query query = resolver.GetQuery(predicate);

            Update<T>(writer, obj, settings, query);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, MappingSettings settings, Expression<Func<T, bool>> predicate, Analyzer analyzer)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == predicate)
            {
                throw new ArgumentNullException("predicate");
            }
            else if (null == analyzer)
            {
                throw new ArgumentNullException("analyzer");
            }

            MappedFieldResolver resolver = settings.ObjectMapper.GetMappedFieldResolver();
            Query query = resolver.GetQuery(predicate);

            Update<T>(writer, obj, settings, query, analyzer);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, Query selection)
        {
            Update<T>(writer, obj, MappingSettings.Default, selection);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, MappingSettings settings, Query selection)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == selection)
            {
                throw new ArgumentNullException("selection");
            }

            writer.DeleteDocuments<T>(selection);
            writer.AddDocument(obj.ToDocument<T>(settings));
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the update operation to.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, DocumentObjectTypeKind kind, Query selection)
        {
            Update<T>(writer, obj, kind, MappingSettings.Default, selection);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the update operation to.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, DocumentObjectTypeKind kind, MappingSettings settings, Query selection)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == selection)
            {
                throw new ArgumentNullException("selection");
            }

            writer.DeleteDocuments<T>(kind, selection);
            writer.AddDocument(obj.ToDocument<T>(settings));
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, Query selection, Analyzer analyzer)
        {
            Update<T>(writer, obj, MappingSettings.Default, selection, analyzer);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, MappingSettings settings, Query selection, Analyzer analyzer)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == selection)
            {
                throw new ArgumentNullException("selection");
            }
            else if (null == analyzer)
            {
                throw new ArgumentNullException("analyzer");
            }

            writer.DeleteDocuments<T>(selection);
            writer.AddDocument(obj.ToDocument<T>(settings), analyzer);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the update operation to.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, DocumentObjectTypeKind kind, Query selection, Analyzer analyzer)
        {
            Update<T>(writer, obj, kind, MappingSettings.Default, selection, analyzer);
        }

        /// <summary>
        /// Updates the specified object in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="obj">
        /// The new object to write.
        /// </param>
        /// <param name="kind">
        /// The kind of type to restrict the update operation to.
        /// </param>
        /// <param name="settings">
        /// The MappingSettings to use when creating the Document to add to the index.
        /// </param>
        /// <param name="selection">
        /// The Query which selects the item in the index.
        /// </param>
        /// <param name="analyzer">
        /// The Analyzer to use.
        /// </param>
        public static void Update<T>(this IndexWriter writer, T obj, DocumentObjectTypeKind kind, MappingSettings settings, Query selection, Analyzer analyzer)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }
            else if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }
            else if (null == selection)
            {
                throw new ArgumentNullException("selection");
            }
            else if (null == analyzer)
            {
                throw new ArgumentNullException("analyzer");
            }

            writer.DeleteDocuments<T>(kind, selection);
            writer.AddDocument(obj.ToDocument<T>(settings), analyzer);
        }

        #endregion

        #region DeleteDocuments

        /// <summary>
        /// Deletes the matching objects in the IndexWriter.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object to update.
        /// </typeparam>
        /// <param name="writer">
        /// The IndexWriter to update the object in.
        /// </param>
        /// <param name="predicate">
        /// The predicate for selecting the item to update.
        /// </param>
        public static void Delete<T>(this IndexWriter writer, Expression<Func<T, bool>> predicate)
        {
            if (null == writer)
            {
                throw new ArgumentNullException("writer");
            }
            else if (null == predicate)
            {
                throw new ArgumentNullException("predicate");
            }

            MappingSettings settings = MappingSettings.Default;
            MappedFieldResolver resolver = settings.ObjectMapper.GetMappedFieldResolver();
            Query query = resolver.GetQuery(predicate);

            DeleteDocuments<T>(writer, query);
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

        #endregion
    }
}
